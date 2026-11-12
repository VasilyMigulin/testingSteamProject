using Game.Shared.Data;
using Game.Shared.Interface;
using System;
using System.Threading; 
using Unity.Netcode;
using UnityEngine;

namespace Game.Net.Connection
{
    // Owns NGO session lifecycle, everything transport specific is delegated to ITransportAdapter 
    public sealed class ConnectionService : IConnectionService, IDisposable
    {
        private readonly NetworkManager _networkManager;
        private readonly ITransportAdapter _transportAdapter;
        private readonly IPlayerIdentityProvider _playerIdentity;
        private readonly NetworkSettings _settings;
        private readonly CancellationTokenSource _lifetimeCts = new();
        private readonly ConnectionApprovalHandler _approval;

        // Filled NGO callbacks while join async is waiting
        private ConnectResult? _pendingJoinResult;
        // Reason captured on disconnect, applied when NGO stop
        private DisconnectReason _pendingDisconnectReason;
 
        public event Action<ConnectionState> StateChanged;

        public bool IsHost { get; private set; } 
        public ConnectionState State { get; private set; } = ConnectionState.Offline;
        public DisconnectReason LastDisconnectReason { get; private set; }

        public ConnectionService(NetworkManager networkManager, ITransportAdapter transport, IPlayerIdentityProvider identity, NetworkSettings settings, ConnectionApprovalHandler approval)
        {
            _networkManager = networkManager;
            _transportAdapter = transport;
            _playerIdentity = identity;
            _settings = settings;
            _approval = approval;

            _networkManager.OnClientConnectedCallback += OnClientConnected;
            _networkManager.OnClientDisconnectCallback += OnClientDisconnect;
            _networkManager.OnClientStopped += OnClientStopped; 
            _transportAdapter.JoinRequested += OnJoinRequested;
        }

        public void Dispose()
        {
            _lifetimeCts.Cancel();
            _lifetimeCts.Dispose();
            _transportAdapter.JoinRequested -= OnJoinRequested;

            // Network manager may already be destroyed when the app is quitting
            if (_networkManager == null) return;

            _networkManager.OnClientConnectedCallback -= OnClientConnected;
            _networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
            _networkManager.OnClientStopped -= OnClientStopped; 
        }

        #region Await

        public async Awaitable<ConnectResult> HostAsync(HostSettings settings, CancellationToken cancellationToken)
        {
            EnsureOffline();
            SetState(ConnectionState.Connecting);

            try
            {
                await WaitForShutdownAsync(cancellationToken);

                ConnectResult prepared = await _transportAdapter.PrepareHostAsync(settings, cancellationToken);

                if (!prepared.Success) return Fail(prepared.FailReason);

                _networkManager.NetworkConfig.ConnectionData = CreatePayload();

                _approval.StartSession(settings.MaxPlayers);

                if (!_networkManager.StartHost()) return Fail(DisconnectReason.StartFailed);

                IsHost = true;
                SetState(ConnectionState.Connected);
                return ConnectResult.Ok();
            }
            catch (OperationCanceledException)
            {
                return Fail(DisconnectReason.None);
            }
        }

        public async Awaitable<ConnectResult> JoinAsync(string address, CancellationToken cancellationToken)
        {
            EnsureOffline();
            SetState(ConnectionState.Connecting);

            try
            {
                await WaitForShutdownAsync(cancellationToken);

                ConnectResult prepared = await _transportAdapter.PrepareClientAsync(address, cancellationToken);

                if (!prepared.Success) return Fail(prepared.FailReason);

                _pendingJoinResult = null;

                _networkManager.NetworkConfig.ConnectionData = CreatePayload();

                if (!_networkManager.StartClient()) return Fail(DisconnectReason.StartFailed);

                ConnectResult result = await WaitForJoinResultAsync(cancellationToken);

                if (!result.Success) return Fail(result.FailReason);

                SetState(ConnectionState.Connected);
                return result;
            }
            catch (OperationCanceledException)
            {
                return Fail(DisconnectReason.None);
            }
        }

        // Polled every frame instead of cancel after timer callbacks run on a thread pool thread, 
        // while NGO and unity API are main thread only
        private async Awaitable<ConnectResult> WaitForJoinResultAsync(CancellationToken ct)
        {
            float deadline = Time.realtimeSinceStartup + _settings.ConnectTimeoutSeconds;

            while (_pendingJoinResult == null)
            {
                if (State != ConnectionState.Connecting)
                    return ConnectResult.Fail(DisconnectReason.None);

                if (Time.realtimeSinceStartup >= deadline)
                    return ConnectResult.Fail(DisconnectReason.Timeout);

                await Awaitable.NextFrameAsync(ct);
            }

            return _pendingJoinResult.Value;
        }

        // A new session can't start until prev NGO shutdown has completed
        private async Awaitable WaitForShutdownAsync(CancellationToken ct)
        {
            while (_networkManager.ShutdownInProgress)
            {
                await Awaitable.NextFrameAsync(ct);
            }
        }

        #endregion

        private void EnsureOffline()
        {
            if (State != ConnectionState.Offline) throw new InvalidOperationException($"Can't start a session because {State}");
        }

        private byte[] CreatePayload() => new ConnectionPayload()
        {
            PlayerId = _playerIdentity.PlayerId,
            PlayerName = _playerIdentity.DisplayName,
            BuildVersion = Application.version
        }.ToBytes();

        private void SetState(ConnectionState state)
        {
            if (State == state) return;

            State = state;

            StateChanged?.Invoke(state);
        }

        public void Leave() => Finish(DisconnectReason.None);

        private ConnectResult Fail(DisconnectReason reason)
        {
            Finish(reason);

            return ConnectResult.Fail(reason);
        }

        // Idempotent can be reached from leave, fail and client stopped for the same session
        private void Finish(DisconnectReason reason)
        {
            if (State == ConnectionState.Offline) return;

            if (_networkManager.IsListening && !_networkManager.ShutdownInProgress) _networkManager.Shutdown();

            _transportAdapter.Cleanup();

            _approval.EndSession();

            IsHost = false;

            LastDisconnectReason = reason;

            _pendingDisconnectReason = DisconnectReason.None;

            SetState(ConnectionState.Offline);
        }

        #region Callbacks
        private void OnClientConnected(ulong clientId)
        {
            if (!_networkManager.IsServer && State == ConnectionState.Connecting)
                _pendingJoinResult = ConnectResult.Ok();
        }

        private void OnClientDisconnect(ulong clientId)
        {
            if (_networkManager.IsServer)
                return;

            DisconnectReason reason = DisconnectReasonCodec.Decode(_networkManager.DisconnectReason);

            if (State == ConnectionState.Connecting)
            {
                _pendingJoinResult = ConnectResult.Fail(reason == DisconnectReason.None ? DisconnectReason.SessionNotFound : reason);
            }
            else
            {
                _pendingDisconnectReason = reason == DisconnectReason.None ? DisconnectReason.HostDisconnected : reason;
            }
        }

        // Single exit point NGO raise this after it has fully stopped for any reason: leave, kick, host lost, transport failure
        // Also fires on the host
        private void OnClientStopped(bool wasHost)
        {
            switch (State)
            {
                case ConnectionState.Offline:
                    return; // Already finish leave or fail
                case ConnectionState.Connecting:

                    _pendingJoinResult ??= ConnectResult.Fail(DisconnectReason.SessionNotFound);
                    return;
                default:
                    Finish(_pendingDisconnectReason);
                    return;
            }
        }

        // Async void is acceptable only for event handlers: nothing can await it, catch exception here
        private async void OnJoinRequested(string address)
        {
            if (State != ConnectionState.Offline)
            {
                Debug.LogWarning($"[Connection] Join request ignore, current state is {State}");
                return;
            }

            try
            {
                await JoinAsync(address, _lifetimeCts.Token);
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }
        #endregion
    }
}
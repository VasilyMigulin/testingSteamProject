using System;
using System.Globalization;
using System.Threading;
using Game.Shared.Data;
using Game.Shared.Interface;
using Netcode.Transports;
using Steamworks;
using Unity.Netcode;
using UnityEngine;

namespace Game.Platform.Steam
{
    // Steam P2P through Steam Datagram Relay: the host is addressed by SteamID, no IP or port forwarding.
    public sealed class SteamTransportAdapter : ITransportAdapter, IDisposable
    { 
        private readonly NetworkManager _networkManager;
        private readonly SteamNetworkingSocketsTransport _transport;
        private readonly SteamLobbyService _lobby;

        public event Action<string> JoinRequested;

        public SteamTransportAdapter(NetworkManager networkManager, SteamNetworkingSocketsTransport transport, SteamLobbyService lobby)
        { 
            _networkManager = networkManager;
            _transport = transport;
            _lobby = lobby; 
            _lobby.JoinRequested += OnLobbyJoinRequested;
        }

        public async Awaitable<ConnectResult> PrepareHostAsync(HostSettings settings, CancellationToken cancellationToken)
        {
            CSteamID? lobby = await _lobby.CreateAsync(settings, cancellationToken);

            if (lobby == null) return ConnectResult.Fail(DisconnectReason.StartFailed);
             
            UseThisTransport();
            return ConnectResult.Ok();
        }
        public async Awaitable<ConnectResult> PrepareClientAsync(string address, CancellationToken cancellationToken)
        { 
            if (!ulong.TryParse(address?.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out ulong lobbyId))
            {
                return ConnectResult.Fail(DisconnectReason.SessionNotFound);
            }
             
            CSteamID? lobby = await _lobby.JoinAsync(new CSteamID(lobbyId), cancellationToken);

            if(lobby == null) return ConnectResult.Fail(DisconnectReason.SessionNotFound);
             
            // The lobby owner is the host: its SteamID is the P2P address.
            _transport.ConnectToSteamID = SteamMatchmaking.GetLobbyOwner(lobby.Value).m_SteamID; 
            UseThisTransport();
            return ConnectResult.Ok();
        }

        // Also called when approval rejects us or the host leaves: we never stay in a lobby without a session.
        public void Cleanup() => _lobby.Leave();
        private void UseThisTransport() => _networkManager.NetworkConfig.NetworkTransport = _transport;
        private void OnLobbyJoinRequested(string lobbyId) => JoinRequested?.Invoke(lobbyId);  
        public void Dispose() => _lobby.JoinRequested -= OnLobbyJoinRequested;
    }
}
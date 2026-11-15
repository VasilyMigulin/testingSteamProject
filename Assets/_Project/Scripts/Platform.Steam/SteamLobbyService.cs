using System;
using System.Threading;
using Game.Shared.Data;
using Game.Shared.Interface;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Game.Platform.Steam
{
    // Steam lobby = matchmaking metadata only (who, where, joinable). Game traffic goes through the P2P transport.
    public sealed class SteamLobbyService : ISessionInvites, IStartable, IDisposable
    {
        private const string LaunchLobbyArgument = "+connect_lobby";
        // AppID 480 is shared by everyone: our own key tells our lobbies apart from other Spacewar tests.
        private const string GameKey = "game";
        private const string GameValue = "artdock-coop";

        private readonly CallResult<LobbyCreated_t> _lobbyCreated;
        private readonly CallResult<LobbyEnter_t> _lobbyEntered;
        private readonly Callback<GameLobbyJoinRequested_t> _joinRequested;

        private AwaitableCompletionSource<CSteamID?> _pendingCreate;
        private AwaitableCompletionSource<CSteamID?> _pendingJoin;
        private HostSettings _pendingSettings;
        private int _maxPlayers;
        private string _launchLobbyId;

        public CSteamID CurrentLobby { get; private set; } = CSteamID.Nil;
         
        public bool CanInvite => CurrentLobby.IsValid();
        public string SessionCode => CurrentLobby.IsValid() ? CurrentLobby.m_SteamID.ToString() : string.Empty; 
        private bool IsOwner() => SteamMatchmaking.GetLobbyOwner(CurrentLobby) == SteamUser.GetSteamID();
        private static ELobbyType ToLobbyType(SessionVisibility visibility) => visibility == SessionVisibility.Private
            ? ELobbyType.k_ELobbyTypePrivate
            : ELobbyType.k_ELobbyTypeFriendsOnly;


        // A lobby id the player wants to join from outside the game UI (friends list, invite).
        public event Action<string> JoinRequested;

        // Takes SteamPlatform only to make the dependency explicit: Steam must be initialized first.
        public SteamLobbyService(SteamPlatform platform)
        {
            _lobbyCreated = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);
            _lobbyEntered = CallResult<LobbyEnter_t>.Create(OnLobbyEntered);
            _joinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequested);
        }


        // "Join Game" while the game is not running starts it with +connect_lobby <id>.
        // The join waits for the menu, otherwise it would race with the bootstrap scene load.
        public void Start()
        {
            string[] args = Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length - 1; i++)
            { 
                if (args[i] == LaunchLobbyArgument) _launchLobbyId = args[i + 1];
            }
             
            if (_launchLobbyId != null) SceneManager.sceneLoaded += OnSceneLoaded;
        }
         
        public async Awaitable<CSteamID?> CreateAsync(HostSettings settings, CancellationToken ct)
        { 
            var source = new AwaitableCompletionSource<CSteamID?>();
            _pendingCreate = source;
            _pendingSettings = settings;
            _lobbyCreated.Set(SteamMatchmaking.CreateLobby(ToLobbyType(settings.Visibility), settings.MaxPlayers));
             
            using (ct.Register(() => source.TrySetCanceled()))
                return await source.Awaitable;
        }

        public async Awaitable<CSteamID?> JoinAsync(CSteamID lobby, CancellationToken ct)
        { 
            var source = new AwaitableCompletionSource<CSteamID?>();
            _pendingJoin = source;
            _lobbyEntered.Set(SteamMatchmaking.JoinLobby(lobby));

            using (ct.Register(() => source.TrySetCanceled()))
                return await source.Awaitable;
        }

        public void Leave()
        {
            if (!CurrentLobby.IsValid()) return;

            // Steam would hand the lobby to another member; closing it first keeps strangers out.
            if (IsOwner()) SteamMatchmaking.SetLobbyJoinable(CurrentLobby, false);
             
            SteamMatchmaking.LeaveLobby(CurrentLobby);
            CurrentLobby = CSteamID.Nil;
        }
         
        public void OpenInviteDialog()
        { 
            if (CanInvite) SteamFriends.ActivateGameOverlayInviteDialog(CurrentLobby);
        }


        // Host only: keeps the Steam lobby consistent with the real session.
        public void UpdateSessionState(int playerCount)
        {
            if (!CurrentLobby.IsValid() || !IsOwner()) return;
             
            SteamMatchmaking.SetLobbyData(CurrentLobby, "players", playerCount.ToString());
            SteamMatchmaking.SetLobbyJoinable(CurrentLobby, playerCount < _maxPlayers);
        }
        #region Callbacks
        private void OnLobbyCreated(LobbyCreated_t result, bool ioFailure)
        { 
            AwaitableCompletionSource<CSteamID?> source = _pendingCreate;
            _pendingCreate = null;
             
            if (ioFailure || result.m_eResult != EResult.k_EResultOK)
            {
                Debug.LogWarning($"[Steam] Lobby creation failed: {result.m_eResult}");
                source?.TrySetResult(null);
                return;
            }
             
            var lobby = new CSteamID(result.m_ulSteamIDLobby);
            CurrentLobby = lobby; 
            _maxPlayers = _pendingSettings.MaxPlayers;
             
            SteamMatchmaking.SetLobbyData(lobby, GameKey, GameValue);
            SteamMatchmaking.SetLobbyData(lobby, "name", _pendingSettings.SessionName);
            SteamMatchmaking.SetLobbyData(lobby, "version", Application.version);

            // Cancelled while Steam was creating it: don't leave an orphan lobby behind.
            if (source == null || !source.TrySetResult(lobby)) Leave();
        }
         
        private void OnLobbyEntered(LobbyEnter_t result, bool ioFailure)
        { 
            AwaitableCompletionSource<CSteamID?> source = _pendingJoin;
            _pendingJoin = null;
             
            var response = (EChatRoomEnterResponse)result.m_EChatRoomEnterResponse;
            if (ioFailure || response != EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
            { 
                Debug.LogWarning($"[Steam] Lobby join failed: {response}");
                source?.TrySetResult(null);
                return;
            } 

            CurrentLobby = new CSteamID(result.m_ulSteamIDLobby);

            if (source == null || !source.TrySetResult(CurrentLobby)) Leave();
        }

        // Friend clicked "Join Game" or accepted an invite while the game is running.
        private void OnJoinRequested(GameLobbyJoinRequested_t request) => JoinRequested?.Invoke(request.m_steamIDLobby.m_SteamID.ToString());

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        { 
            if (scene.name != SceneNames.Menu) return;

            SceneManager.sceneLoaded -= OnSceneLoaded;
            JoinRequested?.Invoke(_launchLobbyId);
        }

        #endregion
        public void Dispose()
        { 
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Leave();
            _lobbyCreated.Dispose();
            _lobbyEntered.Dispose();
            _joinRequested.Dispose();
        }
    }
}
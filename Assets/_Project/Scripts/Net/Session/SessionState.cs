using System.Collections.Generic;
using Game.Net.Connection;
using Game.Shared.Data;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Game.Net.Session
{
    // Session-wide replicated state. Spawned by the host once and kept alive across scene loads.
    public class SessionState : NetworkBehaviour
    {
        // NetworkList must be created before the object is spawned.
        private readonly NetworkList<PlayerNetworkData> _players = new();

        private PlayerRegistry _registry;
        private ConnectionApprovalHandler _approval;

        [Inject]
        public void Construct(PlayerRegistry registry, ConnectionApprovalHandler approval)
        {
            _registry = registry;
            _approval = approval;
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            _players.OnListChanged += OnPlayersChanged;

            if (IsServer)
            {
                // Clients that connected before this object was spawned (at least the host itself).
                foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
                    AddPlayer(clientId);


                NetworkManager.OnClientConnectedCallback += AddPlayer;
                NetworkManager.OnClientDisconnectCallback += RemovePlayer; 
                NetworkManager.SceneManager.OnSceneEvent += OnServerSceneEvent;
            }

            PublishPlayers();
        }

        public override void OnNetworkDespawn()
        {
            _players.OnListChanged -= OnPlayersChanged;

            if (IsServer)
            {
                NetworkManager.OnClientConnectedCallback -= AddPlayer;
                NetworkManager.OnClientDisconnectCallback -= RemovePlayer;

                // SceneManager can already be gone during shutdown.
                if (NetworkManager.SceneManager != null)
                    NetworkManager.SceneManager.OnSceneEvent -= OnServerSceneEvent;
            }

            _registry.Clear();
        }

        // Host-side: replicates who is still loading, so every player can see whom the session waits for.
        private void OnServerSceneEvent(SceneEvent sceneEvent)
        {
            switch (sceneEvent.SceneEventType)
            {
                case SceneEventType.Load when sceneEvent.ClientId == NetworkManager.ServerClientId:
                    SetAllLoading(true); 
                    Debug.Log($"[Session] Loading {sceneEvent.SceneName} started at {Time.realtimeSinceStartup:F2}");
                    break;
                case SceneEventType.LoadComplete:
                    Debug.Log($"[Session] Client {sceneEvent.ClientId} finished loading {sceneEvent.SceneName} at {Time.realtimeSinceStartup:F2}");
                    SetLoading(sceneEvent.ClientId, false);
                    break;

                // Also covers clients that disconnected or timed out during the load.
                case SceneEventType.LoadEventCompleted:
                    SetAllLoading(false);
                    foreach (ulong clientId in sceneEvent.ClientsThatTimedOut)
                        Debug.LogWarning($"[Session] Client {clientId} did not finish loading {sceneEvent.SceneName}");
                    break;

            }
        }

        private void SetLoading(ulong clientId, bool isLoading)
        { 
            for (int i = 0; i < _players.Count; i++)
            {
                if (_players[i].ClientId == clientId)
                {
                    SetLoadingAt(i, isLoading);
                    return;
                }
            }
        }

        private void SetAllLoading(bool isLoading)
        {
            for (int i = 0; i < _players.Count; i++)
                SetLoadingAt(i, isLoading);
        }

        // Elements are structs: copy, modify, write back. Assigning marks the element dirty for replication.
        private void SetLoadingAt(int index, bool isLoading)
        { 
            PlayerNetworkData player = _players[index];

            if (player.IsLoading == isLoading) return;

            player.IsLoading = isLoading;
            _players[index] = player;
        }

        private void AddPlayer(ulong clientId)
        {
            if (!_approval.TryGetPlayer(clientId, out ConnectionPayload payload)) return;

            _players.Add(new PlayerNetworkData(clientId, payload.PlayerId, payload.PlayerName));
        }

        private void RemovePlayer(ulong clientId)
        { 
            for (int i = 0; i < _players.Count; i++)
            {
                if (_players[i].ClientId != clientId) continue;

                _players.RemoveAt(i);
                return;
            }
        }

        private void OnPlayersChanged(NetworkListEvent<PlayerNetworkData> change) => PublishPlayers();

        private void PublishPlayers()
        {
            var players = new List<PlayerInfo>(_players.Count);

            foreach (PlayerNetworkData player in _players)
                players.Add(player.ToInfo(NetworkManager.LocalClientId));
             
            _registry.Set(players);
        }


    }
}
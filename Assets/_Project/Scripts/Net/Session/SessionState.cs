using System.Collections.Generic;
using Game.Net.Connection;
using Game.Shared.Data;
using Unity.Netcode;
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
            }

            _registry.Clear();
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
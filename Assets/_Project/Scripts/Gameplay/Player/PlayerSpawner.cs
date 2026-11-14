using System;
using Unity.Netcode;
using UnityEngine;
using VContainer.Unity;

namespace Game.Gameplay.Player
{
    // Host-only: gives every player an avatar in the game scene, including late joiners.
    public sealed class PlayerSpawner : IStartable, IDisposable
    {
        private readonly NetworkManager _networkManager;
        private readonly PlayerSpawnConfig _config;
        private int _nextSpawnPoint;
        public PlayerSpawner(NetworkManager networkManager, PlayerSpawnConfig config)
        {
            _networkManager = networkManager;
            _config = config;
        }

        public void Start()
        {
            if (!_networkManager.IsServer) return;

            // Clients still loading this scene are fine: NGO defers object creation on them
            // until their load completes (NetworkSceneManager.ShouldDeferCreateObject).
            foreach (ulong clientId in _networkManager.ConnectedClientsIds)
                SpawnFor(clientId);
             
            // Raised after a late joiner has synchronized the current scene.
            _networkManager.OnClientConnectedCallback += SpawnFor;
        }

        // Avatars are player objects: NGO despawns them automatically when the owner disconnects.
        private void SpawnFor(ulong clientId)
        {
            if (_networkManager.ConnectedClients.TryGetValue(clientId, out NetworkClient client) && client.PlayerObject != null) return;
             
            Transform point = _config.GetSpawnPoint(_nextSpawnPoint++);
            _config.PlayerPrefab.InstantiateAndSpawn(
                _networkManager,
                ownerClientId: clientId,
                destroyWithScene: true,
                isPlayerObject: true,
                position: point.position,
                rotation: point.rotation);
        }

        public void Dispose()
        {

            if (_networkManager != null)
                _networkManager.OnClientConnectedCallback -= SpawnFor;
        }
    }
}
using Unity.Netcode;
using UnityEngine;

namespace Game.Gameplay.Player
{
    // Scene data for spawning avatars in the game level.
    public sealed class PlayerSpawnConfig : MonoBehaviour
    {
        [SerializeField] private NetworkObject _playerPrefab;
        [SerializeField] private Transform[] _spawnPoints;

        public NetworkObject PlayerPrefab => _playerPrefab;
        public Transform GetSpawnPoint(int index) => _spawnPoints[index % _spawnPoints.Length];
    }
}
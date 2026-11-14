using Unity.Netcode;
using UnityEngine;

namespace Game.Gameplay.Items
{
    [CreateAssetMenu(menuName = "Game/Item Settings", fileName = "ItemSettings")]
    public sealed class ItemSettings : ScriptableObject
    {
        [SerializeField] private NetworkObject _itemPrefab; 
        [SerializeField, Min(1)] private int _maxItems = 30;
        [SerializeField] private float _grabDistance = 3f;
        // Host sees positions with latency, so its distance check is more forgiving than the client's ray.
        [SerializeField] private float _grabDistanceTolerance = 1.5f; 
        [SerializeField] private float _throwSpeed = 12f;
        [SerializeField] private float _spawnDistance = 1.5f;
        [SerializeField] private float _spawnHeight = 1.5f;

        public NetworkObject ItemPrefab => _itemPrefab;
        public int MaxItems => _maxItems;
        public float GrabDistance => _grabDistance;
        public float HostGrabDistance => _grabDistance + _grabDistanceTolerance;
        public float ThrowSpeed => _throwSpeed;
        public float SpawnDistance => _spawnDistance;
        public float SpawnHeight => _spawnHeight;
    }
}
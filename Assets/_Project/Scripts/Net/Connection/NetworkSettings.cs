using UnityEngine;

namespace Game.Net.Connection
{
    [CreateAssetMenu(menuName = "Game/Network Settings", fileName = "NetworkSettings")]
    public sealed class NetworkSettings : ScriptableObject
    {
        [SerializeField, Min(1f)] private float _connectTimeoutSeconds = 10f;
        public float ConnectTimeoutSeconds => _connectTimeoutSeconds;
    }
}
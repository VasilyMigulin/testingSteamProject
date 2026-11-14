using UnityEngine;

namespace Game.Shared.Data
{
    public readonly struct PlayerInfo
    {
        public readonly ulong ClientId;
        public readonly string PlayerId; 
        public readonly string Name;
        public readonly bool IsHost;
        public readonly bool IsLocal; 
        public readonly bool IsLoading;
        public readonly int PingMs;
        public PlayerInfo(ulong clientId, string playerId, string name, bool isHost, bool isLocal, bool isLoading, int ping)
        {
            ClientId = clientId;
            PlayerId = playerId;
            Name = name;
            IsHost = isHost;
            IsLocal = isLocal;
            IsLoading = isLoading;
            PingMs = ping;
        }
    }
}
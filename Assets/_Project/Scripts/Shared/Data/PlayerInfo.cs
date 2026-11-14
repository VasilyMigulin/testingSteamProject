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
        public PlayerInfo(ulong clientId, string playerId, string name, bool isHost, bool isLocal, bool isLoading)
        {
            ClientId = clientId;
            PlayerId = playerId;
            Name = name;
            IsHost = isHost;
            IsLocal = isLocal;
            IsLoading = isLoading;
        }
    }
}
using System;
using Game.Shared.Data;
using Unity.Collections;
using Unity.Netcode;

namespace Game.Net.Session
{
    // Network representation of a player. NetworkList requires an unmanaged IEquatable type
    // for strings are stored as fixed-size buffers
    public struct PlayerNetworkData : INetworkSerializable, IEquatable<PlayerNetworkData>
    {
        // FixedString64Bytes holds up to 61 UTF-8 bytes.
        private const int MaxNameLength = 24;

        public ulong ClientId;
        public FixedString64Bytes PlayerId;
        public FixedString64Bytes Name;

        public PlayerNetworkData(ulong clientId, string playerId, string name)
        {
            ClientId = clientId;
            PlayerId = new FixedString64Bytes(playerId);
            Name = new FixedString64Bytes(name.Length > MaxNameLength ? name.Substring(0, MaxNameLength) : name);
        }
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PlayerId);
            serializer.SerializeValue(ref Name);
        }

        public bool Equals(PlayerNetworkData other)
        {
            return ClientId == other.ClientId && PlayerId.Equals(other.PlayerId) && Name.Equals(other.Name);
        }

        public PlayerInfo ToInfo(ulong localClientId) => new
        (
            ClientId,
            PlayerId.ToString(),
            Name.ToString(),
            ClientId == NetworkManager.ServerClientId,
            ClientId == localClientId
        );
    }
}

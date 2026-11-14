using Game.Net.Connection;
using Game.Shared.Data;
using Game.Shared.Interface;
using Unity.Netcode;
using UnityEngine;

namespace Game.Net.Session
{
    public sealed class PlayerModeration : IPlayerModeration
    {
        private readonly NetworkManager _networkManager;
        public PlayerModeration(NetworkManager networkManager)
        {
            _networkManager = networkManager;
        }
        public bool CanKick => _networkManager.IsServer;
        public void Kick(ulong clientId)
        {
            if (!CanKick || clientId == NetworkManager.ServerClientId || !_networkManager.ConnectedClients.ContainsKey(clientId)) return;

            // The client decodes the reason and shows "You were kicked" in the menu.
            _networkManager.DisconnectClient(clientId, DisconnectReasonCodec.Encode(DisconnectReason.Kicked));
            Debug.Log($"[Session] Client {clientId} kicked");
        }
    }
}
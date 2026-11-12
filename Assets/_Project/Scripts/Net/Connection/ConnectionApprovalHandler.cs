using System;
using System.Collections.Generic;
using Game.Shared.Data;
using Unity.Netcode;
using UnityEngine;

namespace Game.Net.Connection
{
    // Host gatekeeper: validate every connection request and remember who was let in
    public sealed class ConnectionApprovalHandler : IDisposable
    {
        private readonly NetworkManager _networkManager;
        private readonly Dictionary<ulong, ConnectionPayload> _approvedPlayers = new();
        private int _maxPlayers;

        public ConnectionApprovalHandler(NetworkManager networkManager)
        {
            _networkManager = networkManager;
            _networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        internal bool TryGetPlayer(ulong clientId, out ConnectionPayload payload) => _approvedPlayers.TryGetValue(clientId, out payload); 

        // Called on the host right before start host
        public void StartSession(int maxPlayers)
        {
            _maxPlayers = maxPlayers;
            _approvedPlayers.Clear();
            _networkManager.NetworkConfig.ConnectionApproval = true;
            _networkManager.ConnectionApprovalCallback = Approve;
        }

        public void EndSession()
        {
            _approvedPlayers.Clear();
            _networkManager.ConnectionApprovalCallback = null;
        }

        private void Approve(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            DisconnectReason reject = Validate(request, out ConnectionPayload payload);

            response.Approved = reject == DisconnectReason.None;

            // Avatars spawn in game scene, not on connection (players wait in lobby first)
            response.CreatePlayerObject = false;

            if (response.Approved)
            {
                _approvedPlayers[request.ClientNetworkId] = payload;
                return;
            }

            response.Reason = DisconnectReasonCodec.Encode(reject);

            Debug.Log($"[Approval] Client {request.ClientNetworkId} rejected: {reject}");
        }

        private DisconnectReason Validate(NetworkManager.ConnectionApprovalRequest request, out ConnectionPayload payload)
        {
            // An unreadable payload most likely comes from a build with a different protocol
            if (!ConnectionPayload.TryParse(request.Payload, out payload)) return DisconnectReason.VersionMismatch;

            // NGO run approval for the host own local client as well
            if (request.ClientNetworkId == NetworkManager.ServerClientId) return DisconnectReason.None;

            if (payload.BuildVersion != Application.version) return DisconnectReason.VersionMismatch;

            if (_approvedPlayers.Count >= _maxPlayers) return DisconnectReason.SessionFull;

            if (IsAlreadyConnected(payload.PlayerId)) return DisconnectReason.DuplicatePlayer;

            return DisconnectReason.None;
        }

        private bool IsAlreadyConnected(string playerId)
        {
            foreach (ConnectionPayload player in _approvedPlayers.Values)
            {
                if (player.PlayerId == playerId)
                    return true;
            }
            
            return false;
        } 

        private void OnClientDisconnected(ulong clientId)
        {
            if (_networkManager.IsServer)
                _approvedPlayers.Remove(clientId);
        }

        public void Dispose()
        {
            if (_networkManager != null)
                _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }

    }
}
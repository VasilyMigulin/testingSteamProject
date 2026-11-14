using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Game.Gameplay.Items
{
    // Host-side rules for items. Every client request goes through here; nothing is trusted as-is.
    public sealed class ItemAuthority : IDisposable
    {
        private readonly NetworkManager _networkManager;
        private readonly ItemSettings _settings;
        private readonly HashSet<GrabbableItem> _items = new();
        private readonly Dictionary<ulong, GrabbableItem> _heldItems = new();

        public ItemAuthority(NetworkManager networkManager, ItemSettings settings)
        {
            _networkManager = networkManager;
            _settings = settings;
            _networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        public void Dispose()
        {
            if (_networkManager != null) _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        internal void Register(GrabbableItem item) => _items.Add(item);

        internal void Unregister(GrabbableItem item)
        {
            _items.Remove(item);

            if (item.IsHeld && _heldItems.TryGetValue(item.HolderId, out GrabbableItem held) && held == item)
                _heldItems.Remove(item.HolderId);
        }


        public bool TrySpawn(ulong clientId)
        { 
            if (_items.Count >= _settings.MaxItems) Reject(clientId, "spawn", "item limit reached"); 

            if (!TryGetPlayer(clientId, out Transform player)) return false;

            // Position is computed by the host from its own view of the player, not sent by the client.
            Vector3 position = player.position + player.forward * _settings.SpawnDistance + Vector3.up * _settings.SpawnHeight;

            // Owned by the host: the item does not belong to its creator and outlives them.
            _settings.ItemPrefab.InstantiateAndSpawn(_networkManager, NetworkManager.ServerClientId, destroyWithScene: true, position: position, rotation: Quaternion.identity);

            return true;
        }

        public bool TryGrab(ulong clientId, GrabbableItem item)
        {
            if (item == null || !item.IsSpawned)
            {
                return Reject(clientId, "grab", "item no longer exists");
            }
             
            // Two simultaneous grabs are processed one after another: the second one fails here.
            if (item.IsHeld) return Reject(clientId, "grab", $"already held by {item.HolderId}");

            if (_heldItems.ContainsKey(clientId)) return Reject(clientId, "grab", "hands are full");

            if (!TryGetPlayer(clientId, out Transform player)) return false;
             
            float maxDistance = _settings.HostGrabDistance;

            if ((item.transform.position - player.position).sqrMagnitude > maxDistance * maxDistance) return Reject(clientId, "grab", "too far"); 

            _heldItems[clientId] = item;
            item.ServerGrab(clientId);

            return true;
        }
         
        public bool TryRelease(ulong clientId, Vector3 position, Quaternion rotation, Vector3 velocity)
        {
            if (!_heldItems.Remove(clientId, out GrabbableItem item)) return Reject(clientId, "release", "holds nothing");   

            // Don't trust reported values: keep the start point near the player and clamp the speed.
            if (TryGetPlayer(clientId, out Transform player)
                && (position - player.position).sqrMagnitude > _settings.HostGrabDistance * _settings.HostGrabDistance)
            {
                position = item.transform.position;
            }

            item.ServerRelease(position, rotation, Vector3.ClampMagnitude(velocity, _settings.ThrowSpeed));

            return true;
        }

        public bool TryDelete(ulong clientId)
        {
            if (!_heldItems.Remove(clientId, out GrabbableItem item)) return Reject(clientId, "delete", "holds nothing");   
             
            item.NetworkObject.Despawn();

            return true;
        }

        // The item stays in the world: it is released where the host last saw it and falls.
        private void OnClientDisconnected(ulong clientId)
        {
            if (!_networkManager.IsServer || !_heldItems.Remove(clientId, out GrabbableItem item) || !item.IsSpawned) return;

            item.ServerRelease(item.transform.position, item.transform.rotation, Vector3.zero);
            Debug.Log($"[Items] Client {clientId} disconnected, dropped {item.NetworkObjectId}");
        }
         
        private bool TryGetPlayer(ulong clientId, out Transform player)
        {
            player = null;
            if (_networkManager.ConnectedClients.TryGetValue(clientId, out NetworkClient client) && client.PlayerObject != null)
                player = client.PlayerObject.transform;
             
            return player != null;
        }

        private static bool Reject(ulong clientId, string action, string reason)
        { 
            Debug.Log($"[Items] Client {clientId} {action} rejected: {reason}");
            return false;
        }
    }
}
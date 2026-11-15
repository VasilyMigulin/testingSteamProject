using Game.Gameplay.Items;
using Game.Shared.Data;
using Game.Shared.Interface; 
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Game.Gameplay.Player
{
    // Owner side of item interaction. Sends requests to the host and never changes item state
    // on its own, so a rejected request (lost race, too far) leaves nothing to roll back.
    public class PlayerItemInteractor : NetworkBehaviour
    {
        [SerializeField] private Transform _cameraPivot;
        [SerializeField] private Transform _holdPoint;
        [SerializeField] private LayerMask _itemMask;
         
        private IGameInput _input; 
        private IAchievementService _achievements;
        private ItemAuthority _authority;
        private ItemSettings _settings; 
        private InteractionHintState _hint; 
        private GrabbableItem _focusedItem;

        // Item we asked for; it is ours only once the host confirms (HolderId == our id).
        private GrabbableItem _targetItem;

        private bool IsHolding => _targetItem != null && _targetItem.IsSpawned && _targetItem.HolderId == OwnerClientId;
        
        [Inject]
        public void Construct(IGameInput input, ItemAuthority authority, ItemSettings settings, InteractionHintState hint, IAchievementService achievementService)
        { 
            _input = input;
            _authority = authority;
            _settings = settings;
            _hint = hint;
            _achievements = achievementService;
        } 

        private void Awake() => enabled = false;

        public override void OnNetworkSpawn() => enabled = IsOwner;

        public override void OnNetworkDespawn()
        { 
            SetFocus(null);
            _hint.Set(InteractionHint.None);
            enabled = false;
        }
         
        private void Update()
        {
            SetFocus(IsHolding ? null : FindItemInSight());

            if (_input.SpawnItemPressed) SpawnItemRpc();

            if (_input.InteractPressed)
            {
                if (IsHolding) RequestRelease(Vector3.zero);
                else if (_focusedItem != null) RequestGrab(_focusedItem);
            }
            else if (IsHolding && _input.ThrowPressed)
            {
                RequestRelease(_cameraPivot.forward * _settings.ThrowSpeed);
            }
            else if (IsHolding && _input.DeleteItemPressed)
            { 
                DeleteItemRpc();
                _targetItem = null;
            }

            _hint.Set(IsHolding ? InteractionHint.Holding : _focusedItem != null ? InteractionHint.CanGrab : InteractionHint.None);
        }

        // LateUpdate: after movement and camera rotation, so the item doesn't lag a frame behind the hand.
        // IsOwner: until the ownership change arrives, the host still drives the item.
        private void LateUpdate()
        { 
            if (IsHolding && _targetItem.IsOwner) _targetItem.FollowHoldPoint(_holdPoint);
        }

        private GrabbableItem FindItemInSight()
        { 
            if (!Physics.Raycast(_cameraPivot.position, _cameraPivot.forward, out RaycastHit hit, _settings.GrabDistance, _itemMask)) return null;
             
            GrabbableItem item = hit.collider.GetComponentInParent<GrabbableItem>();

            return item != null && !item.IsHeld ? item : null;
        }

        private void RequestGrab(GrabbableItem item)
        {
            _targetItem = item;
            GrabItemRpc(item.NetworkObject);
        }
         
        private void RequestRelease(Vector3 velocity)
        {
            Transform item = _targetItem.transform;
            ReleaseItemRpc(item.position, item.rotation, velocity);

            // Stop following right away; the host takes over from the reported position. 
            _targetItem = null;
        }

        private void SetFocus(GrabbableItem item)
        { 
            if (item == _focusedItem) return;
             
            if (_focusedItem != null) _focusedItem.SetHighlighted(false);
             
            _focusedItem = item;

            if (_focusedItem != null) _focusedItem.SetHighlighted(true);
        }

        #region RPC

        // Owner-only RPCs: the sender is always OwnerClientId, a client can't act on behalf of another.
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void SpawnItemRpc() => _authority.TrySpawn(OwnerClientId);

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void GrabItemRpc(NetworkObjectReference itemReference)
        {
            GrabbableItem item = itemReference.TryGet(out NetworkObject networkObject)
                ? networkObject.GetComponent<GrabbableItem>()
                : null;

            _authority.TryGrab(OwnerClientId, item);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void ReleaseItemRpc(Vector3 position, Quaternion rotation, Vector3 velocity)
        {
            // A drop is sent with zero velocity; only a confirmed throw counts for stats.
            if (_authority.TryRelease(OwnerClientId, position, rotation, velocity) && velocity != Vector3.zero)
                ItemThrownRpc();
        }

        // The host confirms the event, but only the thrower can write its own Steam stats.
        [Rpc(SendTo.Owner)]
        private void ItemThrownRpc() => _achievements.ReportItemThrown();

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void DeleteItemRpc() => _authority.TryDelete(OwnerClientId);
        #endregion
    }
}
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using VContainer;

namespace Game.Gameplay.Items
{

    // Shared physics item. The host simulates it while free; the holder drives it while held.
    [RequireComponent(typeof(Rigidbody), typeof(NetworkRigidbody))]
    public sealed class GrabbableItem : NetworkBehaviour
    {
        public const ulong NoHolder = ulong.MaxValue;

        [SerializeField] private Collider _collider;
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Color _highlightColor = new(1f, 0.8f, 0.3f);

        // Written only by the host; replicated to everyone, including late joiners.
        private readonly NetworkVariable<ulong> _holderId = new(NoHolder);
         
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private Rigidbody _rigidBody;
        private NetworkRigidbody _networkRigidbody;
        private ItemAuthority _authority;
        private MaterialPropertyBlock _propertyBlock;


        public ulong HolderId => _holderId.Value;
        public bool IsHeld => _holderId.Value != NoHolder;

        [Inject] public void Construct(ItemAuthority authority) => _authority = authority;

        private void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _networkRigidbody = GetComponent<NetworkRigidbody>();
        }

        // Local-only visual feedback for the player aiming at this item; never replicated.
        internal void SetHighlighted(bool highlighted)
        {
            _propertyBlock ??= new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(_propertyBlock);

            if (highlighted)
                _propertyBlock.SetColor(BaseColorId, _highlightColor);
            else
                _propertyBlock.Clear(); 

            _renderer.SetPropertyBlock(_propertyBlock);
        }

        public override void OnNetworkSpawn()
        {
            _holderId.OnValueChanged += OnHolderChanged;
            ApplyPhysicsState();

            if (IsServer) _authority.Register(this);
        }

        public override void OnNetworkDespawn()
        {
            _holderId.OnValueChanged -= OnHolderChanged;


            if (IsServer) _authority.Unregister(this);
        }

        protected override void OnOwnershipChanged(ulong previous, ulong current)
        {
            base.OnOwnershipChanged(previous, current);
            ApplyPhysicsState();
        }

        // Called by the holder every frame: the item follows its hand, the Owner-mode NetworkTransform
        // replicates it to others without any RPC round trip.
        internal void FollowHoldPoint(Transform holdPoint) => transform.SetPositionAndRotation(holdPoint.position, holdPoint.rotation);

        internal void ServerGrab(ulong clientId)
        { 
            _holderId.Value = clientId;
            ApplyPhysicsState();

            if (OwnerClientId != clientId) NetworkObject.ChangeOwnership(clientId);
        } 

        internal void ServerRelease(Vector3 position, Quaternion rotation, Vector3 velocity)
        {
            _holderId.Value = NoHolder;

            if (!IsOwnedByServer) NetworkObject.RemoveOwnership();


            // Start from where the holder saw the item, not from the host's older interpolated copy,
            // otherwise the item would jump back by the latency at the moment of the throw.
            _rigidBody.position = position;
            _rigidBody.rotation = rotation;
            transform.SetPositionAndRotation(position, rotation);

            ApplyPhysicsState();
            _rigidBody.linearVelocity = velocity;
            _rigidBody.angularVelocity = Vector3.zero;
        }

        // Physics runs only on the host and only for a free item. Everywhere else the item is kinematic
        // and moved by NetworkTransform or by the holder's hand, so there is never a second simulation
        // that could disagree with the host and snap on landing. 
        private void ApplyPhysicsState()
        {
            bool simulate = IsServer && !IsHeld;
            _networkRigidbody.SetIsKinematic(!simulate);

            // A held item has no collider, so it doesn't push its own holder or snag on walls.
            _collider.enabled = !IsHeld;
        }

        private void OnHolderChanged(ulong previous, ulong current) => ApplyPhysicsState();
    }
}
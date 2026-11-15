using Game.Shared.Interface;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Game.Gameplay.Player
{
    // Avatar color. The owner asks, the host picks the next palette entry, everyone (including late joiners) applies it.
    public sealed class PlayerAppearance : NetworkBehaviour
    {
        // URP Lit base color.
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Renderer _bodyRenderer;
        [SerializeField] private Color[] _palette =
       {
            new(0.9f, 0.3f, 0.3f),
            new(0.3f, 0.6f, 0.9f),
            new(0.4f, 0.8f, 0.4f),
            new(0.95f, 0.8f, 0.3f),
            new(0.7f, 0.4f, 0.9f),
            new(0.95f, 0.55f, 0.2f)
        };

        // A palette index instead of a Color: 1 byte on the wire, and only valid colors can be chosen.
        private readonly NetworkVariable<byte> _colorIndex = new();

        private IGameInput _input;
        private MaterialPropertyBlock _propertyBlock;

        [Inject] public void Construct(IGameInput input) => _input = input;

        public override void OnNetworkSpawn()
        {
            if (IsServer) _colorIndex.Value = (byte)(OwnerClientId % (ulong)_palette.Length);

            _colorIndex.OnValueChanged += OnColorChanged;
            ApplyColor(_colorIndex.Value);
        }

        public override void OnNetworkDespawn() => _colorIndex.OnValueChanged -= OnColorChanged;

        private void Update()
        {
            if (IsOwner && _input.ChangeColorPressed) RequestNextColorRpc(); 
        }


        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void RequestNextColorRpc() => _colorIndex.Value = (byte)((_colorIndex.Value + 1) % _palette.Length);

        private void OnColorChanged(byte previous, byte current) => ApplyColor(current);

        private void ApplyColor(byte index)
        { 
            _propertyBlock ??= new MaterialPropertyBlock();
            _bodyRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(BaseColorId, _palette[index % _palette.Length]);
            _bodyRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
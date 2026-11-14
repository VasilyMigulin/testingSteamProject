using Game.Shared.Interface;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using VContainer;

namespace Game.Gameplay.Player
{
    // Owner-authoritative first-person controller. Runs only on the owning peer;
    // everyone else receives the result through the Owner-mode NetworkTransform.
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private Transform _cameraPivot;
        [SerializeField] private GameObject _cameraRoot;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _jumpHeight = 1.2f;
        [SerializeField] private float _gravity = -20f;
        [SerializeField] private float _lookSensitivity = 0.1f;
        [SerializeField] private float _maxPitch = 85f;


        // Small downward speed keeps the controller snapped to the ground on slopes.
        private const float GroundedVerticalSpeed = -2f;

        private CharacterController _controller;
        private IGameInput _input;
        private float _verticalSpeed;
        private float _pitch;

        [Inject] public void Construct(IGameInput input) => _input = input;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _cameraRoot.SetActive(false);
            enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            _cameraRoot.SetActive(IsOwner);
            enabled = IsOwner;
        }

        public override void OnNetworkDespawn() => enabled = false;

        private void Update()
        {
            Look(_input.Look);
            Move(_input.Move, _input.JumpPressed);
        }

        // Mouse delta is already per-frame, so it is not scaled by deltaTime.
        private void Look(Vector2 delta)
        {
            transform.Rotate(0f, delta.x * _lookSensitivity, 0f);

            // Pitch stays local: other players don't need it, so it is never sent.
            _pitch = Mathf.Clamp(_pitch - delta.y * _lookSensitivity, -_maxPitch, _maxPitch);
            _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void Move(Vector2 input, bool jump)
        { 
            if (_controller.isGrounded && _verticalSpeed < 0f) _verticalSpeed = GroundedVerticalSpeed;
             
            if (jump && _controller.isGrounded) _verticalSpeed = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
             
            _verticalSpeed += _gravity * Time.deltaTime; 

            Vector3 horizontal = (transform.right * input.x + transform.forward * input.y) * _moveSpeed;
            _controller.Move((horizontal + Vector3.up * _verticalSpeed) * Time.deltaTime);
        }
    }
}
using System;
using Game.Shared.Interface;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Gameplay.Input
{
    public sealed class GameInput : IGameInput, IDisposable
    {
        private readonly InputActionMap _playerMap;
        private readonly InputAction _move;
        private readonly InputAction _look;
        private readonly InputAction _jump;
        private readonly InputAction _playerList;
        private readonly InputAction _pause;
        private readonly InputAction _interact;
        private readonly InputAction _throw;
        private readonly InputAction _spawnItem;
        private readonly InputAction _deleteItem;
        private bool _gameplayEnabled;

        public GameInput(InputActionAsset actions)
        {
            _playerMap = actions.FindActionMap("Player", throwIfNotFound: true);
            _move = _playerMap.FindAction("Move", throwIfNotFound: true);
            _look = _playerMap.FindAction("Look", throwIfNotFound: true);
            _jump = _playerMap.FindAction("Jump", throwIfNotFound: true);
            _playerList = _playerMap.FindAction("PlayerList", throwIfNotFound: true);
            _pause = _playerMap.FindAction("Pause", throwIfNotFound: true); 
            _interact = _playerMap.FindAction("Interact", throwIfNotFound: true);
            _throw = _playerMap.FindAction("Attack", throwIfNotFound: true);
            _spawnItem = _playerMap.FindAction("SpawnItem", throwIfNotFound: true);
            _deleteItem = _playerMap.FindAction("DeleteItem", throwIfNotFound: true);
            _playerMap.Enable();
        }

        public Vector2 Move => _gameplayEnabled ? _move.ReadValue<Vector2>() : Vector2.zero;
        public Vector2 Look => _gameplayEnabled ? _look.ReadValue<Vector2>() : Vector2.zero;
        public bool JumpPressed => _gameplayEnabled && _jump.WasPressedThisFrame();
        public bool PlayerListHeld => _playerList.IsPressed();
        public bool PausePressed => _pause.WasPressedThisFrame();
        public bool InteractPressed => _gameplayEnabled && _interact.WasPressedThisFrame();
        public bool ThrowPressed => _gameplayEnabled && _throw.WasPressedThisFrame();
        public bool SpawnItemPressed => _gameplayEnabled && _spawnItem.WasPressedThisFrame();
        public bool DeleteItemPressed => _gameplayEnabled && _deleteItem.WasPressedThisFrame();

        public void SetGameplayEnabled(bool enabled)
        {
            _gameplayEnabled = enabled;
            Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !enabled;
        }
        public void Dispose() => _playerMap.Disable();
    }
}
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

        private bool _gameplayEnabled;

        public GameInput(InputActionAsset actions)
        {
            _playerMap = actions.FindActionMap("Player", throwIfNotFound: true);
            _move = _playerMap.FindAction("Move", throwIfNotFound: true);
            _look = _playerMap.FindAction("Look", throwIfNotFound: true);
            _jump = _playerMap.FindAction("Jump", throwIfNotFound: true);
            _playerList = _playerMap.FindAction("PlayerList", throwIfNotFound: true);
            _pause = _playerMap.FindAction("Pause", throwIfNotFound: true);
            _playerMap.Enable();
        }

        public Vector2 Move => _gameplayEnabled ? _move.ReadValue<Vector2>() : Vector2.zero;
        public Vector2 Look => _gameplayEnabled ? _look.ReadValue<Vector2>() : Vector2.zero;
        public bool JumpPressed => _gameplayEnabled && _jump.WasPressedThisFrame();
        public bool PlayerListHeld => _playerList.IsPressed();
        public bool PausePressed => _pause.WasPressedThisFrame();
         
        public void SetGameplayEnabled(bool enabled)
        {
            _gameplayEnabled = enabled;
            Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !enabled;
        }
        public void Dispose() => _playerMap.Disable();
    }
}
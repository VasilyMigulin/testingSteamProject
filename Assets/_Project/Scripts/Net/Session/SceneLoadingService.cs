using System;
using Game.Shared.Interface;
using Unity.Netcode;
using UnityEngine;
using VContainer.Unity;

namespace Game.Net.Session
{
    // Tracks whether the local player is loading a network scene.
    public class SceneLoadingService : ILoadingState, IStartable, IDisposable
    {
        private readonly NetworkManager _networkManager;

        public bool IsLoading { get; private set; }

        public event Action<bool> LoadingChanged;
        public SceneLoadingService(NetworkManager networkManager)
        {
            _networkManager = networkManager;
        }

        public void Start()
        {
            // SceneManager exists only after NGO starts; OnClientStarted fires before the initial
            // synchronization, so a late joiner sees its loading screen from the very beginning.
            _networkManager.OnClientStarted += OnClientStarted;
            _networkManager.OnClientStopped += OnClientStopped;
        }

        public void Dispose()
        { 
            if (_networkManager == null)
                return; 

            _networkManager.OnClientStarted -= OnClientStarted;
            _networkManager.OnClientStopped -= OnClientStopped;
        }

        private void OnClientStarted()
        {
            _networkManager.SceneManager.OnSceneEvent += OnSceneEvent;
        }

        private void OnClientStopped(bool wasHost)
        { 
            if (_networkManager.SceneManager != null)
                _networkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
             
            SetLoading(false);
        }

        private void OnSceneEvent(SceneEvent sceneEvent)
        {
            // Scene events are the main source of truth when debugging loading and late join.
            if (sceneEvent.ClientId == _networkManager.LocalClientId)
                Debug.Log($"[Scene] {sceneEvent.SceneEventType} {sceneEvent.SceneName}");

            // The host receives events about every client; only the local ones matter here.
            bool isLocal = sceneEvent.ClientId == _networkManager.LocalClientId;
             
            switch (sceneEvent.SceneEventType)
            {
                case SceneEventType.Load when isLocal:
                case SceneEventType.Synchronize when isLocal:
                    SetLoading(true);
                    break;

                // Sent by the host to everyone once all clients finished, disconnected or timed out,
                // so the loading screen hides for all players at the same moment.
                case SceneEventType.LoadEventCompleted:
                    SetLoading(false);
                    break;

                // Late join: only this client was loading, nobody else waits for it.
                case SceneEventType.SynchronizeComplete when isLocal:
                    SetLoading(false);
                    break; 
            }
        }

        private void SetLoading(bool isLoading)
        {
            if (IsLoading == isLoading) return;

            IsLoading = isLoading;
            LoadingChanged?.Invoke(isLoading);
        }
    }
}
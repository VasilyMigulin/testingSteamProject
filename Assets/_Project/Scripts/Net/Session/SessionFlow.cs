using System;
using Game.Net.Connection;
using Game.Shared.Data;
using Game.Shared.Interface;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Game.Net.Session
{
    // Drives scene transitions for the session lifecycle: menu -> lobby -> game -> menu.
    public sealed class SessionFlow : ISessionFlow, IStartable, IDisposable
    {
        private readonly NetworkManager _networkManager;
        private readonly IConnectionService _connection;
        private readonly NetworkSettings _settings;

        public SessionFlow(NetworkManager networkManager, IConnectionService connection, NetworkSettings settings)
        {
            _networkManager = networkManager;
            _connection = connection;
            _settings = settings;
        }

        public void Start() => _connection.StateChanged += OnConnectionStateChanged;
        public void Dispose() => _connection.StateChanged -= OnConnectionStateChanged;
        public void StartGame()
        {
            if (!_networkManager.IsServer)
                return;

            LoadNetworkScene(SceneNames.Game);
        }

        private void OnConnectionStateChanged(ConnectionState state)
        {
            switch (state)
            {
                case ConnectionState.Connected when _connection.IsHost:
                    StartSession();
                    break;
                case ConnectionState.Offline:
                    ReturnToMenu();
                    break;
            }
        } 
        
        // Clients don't load the lobby themselves: NGO synchronizes the host's scene on connect.
        private void StartSession()
        {
            var prefab = _settings.SessionStatePrefab.GetComponent<NetworkObject>();
            prefab.InstantiateAndSpawn(_networkManager, destroyWithScene: false);

            LoadNetworkScene(SceneNames.Lobby);
        }

        private static void ReturnToMenu()
        {
            if (SceneManager.GetActiveScene().name != SceneNames.Menu) SceneManager.LoadScene(SceneNames.Menu); 
        }

        private void LoadNetworkScene(string sceneName)
        {
            SceneEventProgressStatus status = _networkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            if (status != SceneEventProgressStatus.Started)
                Debug.LogWarning($"[Session] Failed to start loading {sceneName}: {status}");
        }
    }
}
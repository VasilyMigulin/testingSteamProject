using System;
using Game.Shared.Data;
using Game.Shared.Interface;
using Steamworks;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Game.Platform.Steam
{
    // Friends see where the player is: menu, lobby or game with the player count.
    public sealed class SteamRichPresence : IStartable, IDisposable
    {
        private readonly IConnectionService _connection;
        private readonly IPlayerRegistry _players;
        private readonly SteamLobbyService _lobby;

        public SteamRichPresence(IConnectionService connection, IPlayerRegistry players, SteamLobbyService lobby)
        {
            _connection = connection;
            _players = players;
            _lobby = lobby; 
        }

        public void Start()
        {
            _connection.StateChanged += OnConnectionStateChanged;
            _players.Changed += Refresh;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            Refresh();

        }

        private void Refresh()
        {  
            if (_connection.State != ConnectionState.Connected || !_lobby.CurrentLobby.IsValid())
            { 
                SteamFriends.ClearRichPresence();
                SteamFriends.SetRichPresence("status", "In menu");
                return;
            }

            int players = _players.Players.Count;
            string place = SceneManager.GetActiveScene().name == SceneNames.Game ? "In game" : "In lobby";
            SteamFriends.SetRichPresence("status", $"{place} {players}/{_lobby.MaxPlayers}");

            // Groups friends playing in the same session in the Steam friends list.
            SteamFriends.SetRichPresence("steam_player_group", _lobby.SessionCode);
            SteamFriends.SetRichPresence("steam_player_group_size", players.ToString());
        }

        private void OnConnectionStateChanged(ConnectionState state) => Refresh();
        private void OnActiveSceneChanged(Scene previous, Scene next) => Refresh();

        public void Dispose()
        {
            _connection.StateChanged -= OnConnectionStateChanged;
            _players.Changed -= Refresh; 
            SceneManager.activeSceneChanged -= OnActiveSceneChanged; 
            SteamFriends.ClearRichPresence();
        }
    }
}
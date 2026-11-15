using System;
using Game.Shared.Interface;
using VContainer.Unity;

namespace Game.Platform.Steam
{
    // Mirrors the session into the Steam lobby: player count and whether it is joinable.
    public sealed class SteamLobbySync : IStartable, IDisposable
    {
        private readonly IPlayerRegistry _players;
        private readonly SteamLobbyService _lobby;
         
        public SteamLobbySync(IPlayerRegistry players, SteamLobbyService lobby)
        { 
            _players = players;
            _lobby = lobby;
        }
         
        public void Start() => _players.Changed += OnPlayersChanged;

        public void Dispose() => _players.Changed -= OnPlayersChanged;


        // Ignored on clients: only the lobby owner can write lobby data.
        private void OnPlayersChanged() => _lobby.UpdateSessionState(_players.Players.Count);
    }
}
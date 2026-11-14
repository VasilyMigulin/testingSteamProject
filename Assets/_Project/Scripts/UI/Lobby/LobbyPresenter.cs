using Game.Shared.Data;
using Game.Shared.Interface;
using Game.UI.Common;
using System;
using System.Collections.Generic;
using VContainer.Unity;

namespace Game.UI.Lobby
{
    public sealed class LobbyPresenter : IStartable, IDisposable
    {
        private readonly LobbyView _view;
        private readonly IPlayerRegistry _players;
        private readonly IConnectionService _connection;
        private readonly ISessionFlow _session;
        public LobbyPresenter(LobbyView view, IPlayerRegistry players, IConnectionService connection, ISessionFlow session)
        {
            _view = view;
            _players = players;
            _connection = connection;
            _session = session;
        }

        public void Start()
        {
            _view.StartClicked += _session.StartGame;
            _view.LeaveClicked += _connection.Leave; 
            _players.Changed += Render;

            _view.SetCanStart(_connection.IsHost);
            Render();
        } 

        private void Render()
        {
            var lines = new List<string>(_players.Players.Count);
            foreach (PlayerInfo player in _players.Players)
                lines.Add(PlayerLabel.Format(player));

            _view.SetPlayers(lines); 
        } 

        public void Dispose()
        {
            _view.StartClicked -= _session.StartGame;
            _view.LeaveClicked -= _connection.Leave;
            _players.Changed -= Render;
        }
    }
}
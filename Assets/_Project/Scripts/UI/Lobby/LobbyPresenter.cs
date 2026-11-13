using System;
using System.Collections.Generic;
using Game.Shared.Data;
using Game.Shared.Interface;
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
                lines.Add(Format(player));

            _view.SetPlayers(lines); 
        }

        private static string Format(PlayerInfo playerInfo)
        {
            string line = playerInfo.Name;

            if (playerInfo.IsHost)
            {
                line += " (host)";
            }

            if (playerInfo.IsLocal)
            {
                line += " (you)";
            }

            return line;
        }

        public void Dispose()
        {
            _view.StartClicked -= _session.StartGame;
            _view.LeaveClicked -= _connection.Leave;
            _players.Changed -= Render;
        }
    }
}
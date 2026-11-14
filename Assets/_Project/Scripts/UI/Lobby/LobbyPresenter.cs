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
        private readonly PlayerListView _playerListView;
        private readonly LobbyView _view;
        private readonly IPlayerModeration _moderation;
        private readonly IPlayerRegistry _players;
        private readonly IConnectionService _connection;
        private readonly ISessionFlow _session;
        public LobbyPresenter(LobbyView view, IPlayerRegistry players, IConnectionService connection, ISessionFlow session, PlayerListView playerListView, IPlayerModeration moderation)
        {
            _view = view;
            _players = players;
            _connection = connection;
            _session = session;
            _playerListView = playerListView;
            _moderation = moderation;
        }

        public void Start()
        {
            _view.StartClicked += _session.StartGame;
            _view.LeaveClicked += _connection.Leave; 
            _players.Changed += Render;
            _playerListView.KickClicked += _moderation.Kick;

            _view.SetCanStart(_connection.IsHost);
            Render();
        }

        private void Render() => _playerListView.Render(PlayerRows.Build(_players.Players, _moderation.CanKick));

        public void Dispose()
        {
            _view.StartClicked -= _session.StartGame;
            _playerListView.KickClicked -= _moderation.Kick;
            _view.LeaveClicked -= _connection.Leave;
            _players.Changed -= Render;
        }
    }
}
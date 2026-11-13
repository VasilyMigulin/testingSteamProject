using Game.Shared.Data;
using Game.Shared.Interface;
using System;
using System.Threading;
using UnityEngine;
using VContainer.Unity; 

namespace Game.UI.Menu
{
    public sealed class MenuPresenter : IStartable, IDisposable
    {
        private const int DefaultMaxPlayers = 4;
        private readonly MenuView _view;
        private readonly IConnectionService _connection; 

        public MenuPresenter(MenuView view, IConnectionService connection)
        {
            _view = view;
            _connection = connection;
        }

        public void Start()
        {
            _view.HostClicked += OnHostClicked;
            _view.JoinClicked += OnJoinClicked;
            _view.LeaveClicked += OnLeaveClicked;
            _connection.StateChanged += Render;

            Render(_connection.State);
        }

        public void Dispose()
        { 
            _view.HostClicked -= OnHostClicked;
            _view.JoinClicked -= OnJoinClicked;
            _view.LeaveClicked -= OnLeaveClicked;
            _connection.StateChanged -= Render;
        }

        private async void OnHostClicked()
        {
            try
            {
                var settings = new HostSettings("Session", DefaultMaxPlayers, SessionVisibility.FriendsOnly);
                await _connection.HostAsync(settings, CancellationToken.None); 
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async void OnJoinClicked(string address)
        {
            try
            {
                await _connection.JoinAsync(address, CancellationToken.None);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        } 

        private void OnLeaveClicked() => _connection.Leave();

        private void Render(ConnectionState state)
        {
            _view.SetCanStart(state == ConnectionState.Offline);
            _view.SetStatus(state switch
            {
                ConnectionState.Connecting => "Connecting...",
                ConnectionState.Connected => _connection.IsHost ? "Hosting" : "Connected",
                _ => DisconnectReasonText.Get(_connection.LastDisconnectReason)
            });
        }
    }
}
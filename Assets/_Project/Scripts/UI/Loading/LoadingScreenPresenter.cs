using System;
using System.Text;
using Game.Shared.Data;
using Game.Shared.Interface;
using VContainer.Unity;

namespace Game.UI.Loading
{
    public sealed class LoadingScreenPresenter : IStartable, IDisposable
    {
        private readonly LoadingScreenView _view;
        private readonly ILoadingState _loading;
        private readonly IPlayerRegistry _players;

        public LoadingScreenPresenter(LoadingScreenView view, ILoadingState loading, IPlayerRegistry players)
        {
            _view = view;
            _loading = loading;
            _players = players;
        }

        public void Start()
        {
            _loading.LoadingChanged += OnLoadingChanged; 
            _players.Changed += RenderWaiting;

            OnLoadingChanged(_loading.IsLoading);
        }

        private void OnLoadingChanged(bool isLoading)
        {
            _view.SetVisible(isLoading);
            RenderWaiting();
        }

        private void RenderWaiting()
        {
            if (!_loading.IsLoading) return;

            var text = new StringBuilder("Loading...");

            foreach (PlayerInfo player in _players.Players)
            {
                if (player.IsLoading)
                {
                    text.Append('\n').Append("Waiting for ").Append(player.Name);
                }
            }

            _view.SetWaiting(text.ToString());
        }

        public void Dispose()
        {
            _loading.LoadingChanged -= OnLoadingChanged;
            _players.Changed -= RenderWaiting;
        }
    }
}
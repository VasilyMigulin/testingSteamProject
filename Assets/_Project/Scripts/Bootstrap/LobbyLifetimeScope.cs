using Game.UI.Lobby;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Bootstrap
{
    public sealed class LobbyLifetimeScope : LifetimeScope
    {
        [SerializeField] private LobbyView _lobbyView;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_lobbyView);
            builder.RegisterEntryPoint<LobbyPresenter>();
        }
    }
}
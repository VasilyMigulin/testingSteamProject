using Game.Gameplay.Player;
using Game.UI.Game;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Bootstrap
{
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private PlayerSpawnConfig _spawnConfig;
        [SerializeField] private GameHudView _hudView;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_spawnConfig);
            builder.RegisterEntryPoint<PlayerSpawner>(); 

            builder.RegisterComponent(_hudView);
            builder.RegisterEntryPoint<GameHudPresenter>();
        }
    }
}
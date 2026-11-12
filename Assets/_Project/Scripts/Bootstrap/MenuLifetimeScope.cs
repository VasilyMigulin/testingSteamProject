using Game.UI.Menu;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Bootstrap
{
    public class MenuLifetimeScope : LifetimeScope
    {
        [SerializeField] private MenuView _menuView;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_menuView);
            builder.RegisterEntryPoint<MenuPresenter>();
        }
    }
}
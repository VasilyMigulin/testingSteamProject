using Game.Net.Connection;
using Game.Platform.Local;
using Game.Shared.Interface;
using Game.Transport.Utp;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Bootstrap
{ 
    // Composition root: the only place that knows concrete transports and platforms.
    public sealed class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private NetworkManager _networkManager; 
        [SerializeField] private UnityTransport _unityTransport;
        [SerializeField] private NetworkSettings _networkSettings;

        protected override void Awake()
        {
            // Lives for the whole session; scene scopes use it as their parent 
            DontDestroyOnLoad(gameObject);
            base.Awake();
        } 
        
        protected override void Configure(IContainerBuilder builder)
        {
            LaunchOptions options = LaunchOptions.FromCommandLine();
            builder.RegisterInstance(options);

            builder.RegisterComponent(_networkManager);
            builder.RegisterInstance(_networkSettings);

            RegisterPlatform(builder, options);
            RegisterTransport(builder, options);

            builder.Register<ConnectionApprovalHandler>(Lifetime.Singleton);
            builder.Register<ConnectionService>(Lifetime.Singleton).As<IConnectionService>();

            builder.RegisterEntryPoint<BootstrapEntryPoint>();
        }

        private static void RegisterPlatform(IContainerBuilder builder, LaunchOptions options)
        {
            builder.Register<GuidIdentityProvider>(Lifetime.Singleton)
                .As<IPlayerIdentityProvider>()
                .WithParameter("displayNameOverride", options.PlayerName);
        }

        private void RegisterTransport(IContainerBuilder builder, LaunchOptions options)
        {
            builder.RegisterComponent(_unityTransport);
            builder.Register<UtpTransportAdapter>(Lifetime.Singleton)
                .As<ITransportAdapter>()
                .WithParameter(options.Port);
        }
    }
}
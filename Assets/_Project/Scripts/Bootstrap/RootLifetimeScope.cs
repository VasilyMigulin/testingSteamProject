using Game.Gameplay.Input;
using Game.Gameplay.Items;
using Game.Net.Connection;
using Game.Net.Injection;
using Game.Net.Session;
using Game.Platform.Local;
using Game.Platform.Steam;
using Game.Shared.Interface;
using Game.Transport.Utp;
using Game.UI.Loading;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.InputSystem;
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
        [SerializeField] private LoadingScreenView _loadingScreenView;
        [SerializeField] private InputActionAsset _inputActions;
        [SerializeField] private ItemSettings _itemSettings;

        // Multiplayer Play Mode instances share one Steam account and would be rejected as duplicates,
        // so the editor defaults to direct IP while builds default to Steam. -transport overrides both.
        [SerializeField] private TransportKind _editorTransport = TransportKind.UnityTransport; 
        [SerializeField] private TransportKind _buildTransport = TransportKind.Steam;
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

            bool useSteam = TryInitSteam(options, out SteamPlatform steam, out string notice);
            builder.RegisterInstance<IPlatformInfo>(new PlatformInfo(useSteam, notice));

            if (useSteam)
                SteamInstaller.Install(builder, steam, _networkManager);
            else
            { 
                RegisterLocalPlatform(builder, options);
                RegisterTransport(builder, options);
            } 


            builder.Register<ConnectionApprovalHandler>(Lifetime.Singleton);
            builder.Register<ItemAuthority>(Lifetime.Singleton);
            builder.Register<ConnectionService>(Lifetime.Singleton).As<IConnectionService>(); 
            builder.Register<PlayerRegistry>(Lifetime.Singleton).AsSelf().As<IPlayerRegistry>();
            builder.Register<InteractionHintState>(Lifetime.Singleton).AsSelf().As<IInteractionHint>(); 
            builder.Register<PlayerModeration>(Lifetime.Singleton).As<IPlayerModeration>();
            builder.Register<GameInput>(Lifetime.Singleton).As<IGameInput>().WithParameter(_inputActions);
            builder.RegisterEntryPoint<SessionFlow>();
            builder.RegisterEntryPoint<SceneLoadingService>();
            builder.RegisterComponent(_loadingScreenView);
            builder.RegisterInstance(_itemSettings);
            builder.RegisterEntryPoint<LoadingScreenPresenter>();
            builder.RegisterEntryPoint<NetworkPrefabInjector>();  

            builder.RegisterEntryPoint<BootstrapEntryPoint>();
        }

        private bool TryInitSteam(LaunchOptions options, out SteamPlatform steam, out string notice)
        {
            steam = null;
            notice = string.Empty;

            TransportKind preferred = options.Transport ?? (Application.isEditor ? _editorTransport : _buildTransport);

            if (preferred != TransportKind.Steam) return false;

            var platform = new SteamPlatform();

            if (platform.TryInit(out string error))
            {
                steam = platform;
                return true;
            }

            // Required by the task: without Steam the build must not crash, it falls back to direct IP.
            notice = $"{error}. Using direct IP connect";
            Debug.LogWarning($"[Steam] {notice}");
            return false;
        }
         
        private static void RegisterLocalPlatform(IContainerBuilder builder, LaunchOptions options)
        {
            builder.Register<GuidIdentityProvider>(Lifetime.Singleton)
                .As<IPlayerIdentityProvider>()
                .WithParameter("displayNameOverride", options.PlayerName);

            builder.Register<LogAchievementService>(Lifetime.Singleton).As<IAchievementService>();
        }

        private void RegisterTransport(IContainerBuilder builder, LaunchOptions options)
        {
            builder.RegisterComponent(_unityTransport);
            builder.Register<UtpTransportAdapter>(Lifetime.Singleton)
                .As<ITransportAdapter>()
                .WithParameter(options.Port); 
            builder.Register<UtpSessionInvites>(Lifetime.Singleton).As<ISessionInvites>().WithParameter(options.Port);
        }
    }
}
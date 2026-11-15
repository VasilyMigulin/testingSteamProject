using Game.Shared.Interface;
using Netcode.Transports;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Platform.Steam
{
    public static class SteamInstaller
    {
        public static void Install(IContainerBuilder builder, SteamPlatform steam, NetworkManager networkManager)
        {
            if (!networkManager.TryGetComponent(out SteamNetworkingSocketsTransport transport))
                throw new MissingComponentException($"{nameof(SteamNetworkingSocketsTransport)} is missing on the NetworkManager object.");

            builder.RegisterInstance(steam);
            builder.RegisterEntryPoint<SteamCallbackPump>();
            builder.Register<SteamIdentityProvider>(Lifetime.Singleton).As<IPlayerIdentityProvider>();

            builder.RegisterInstance(transport); 
            builder.RegisterEntryPoint<SteamLobbyService>().AsSelf();
            builder.Register<SteamTransportAdapter>(Lifetime.Singleton).As<ITransportAdapter>();
            builder.RegisterEntryPoint<SteamLobbySync>();
        }
    }
}
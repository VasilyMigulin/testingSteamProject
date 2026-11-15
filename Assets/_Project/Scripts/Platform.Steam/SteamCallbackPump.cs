using System;
using VContainer.Unity;

namespace Game.Platform.Steam
{
    // Pumps Steam callbacks every frame and shuts Steam down with the container.
    // A separate entry point because instances registered with RegisterInstance are not disposed by VContainer.
    public sealed class SteamCallbackPump : ITickable, IDisposable
    { 
        private readonly SteamPlatform _platform;
        public SteamCallbackPump(SteamPlatform platform)
        {
            _platform = platform;
        }

        public void Tick() => _platform.RunCallbacks();
        public void Dispose() => _platform.Dispose();
    }
}
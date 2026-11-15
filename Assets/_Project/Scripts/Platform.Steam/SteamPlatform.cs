using System;
using Steamworks;

namespace Game.Platform.Steam
{
    // Owns the Steam API lifetime. Init can fail (Steam not running) the caller decides what to fall back to
    public sealed class SteamPlatform : IDisposable
    {
        public bool IsInit { get; private set; }
        public bool TryInit(out string error)
        {
            error = null;
             
            // Sanity checks from Steamworks.NET: wrong native library or struct packing would crash later in random places.
            if (!Packsize.Test())
            {
                error = "Steamworks.NET packsize mismatch";
                return false;
            }

            if (!DllCheck.Test())
            { 
                error = "steam_api64.dll version mismatch";
                return false;
            }

            try
            {
                IsInit = SteamAPI.Init();
            }
            catch(DllNotFoundException e)
            { 
                error = $"steam_api64.dll not found ({e.Message})";
                return false;
            }

            if(!IsInit) error = "Steam is not running";

            return IsInit;
        }

        public void RunCallbacks()
        {
            if (IsInit) SteamAPI.RunCallbacks();
        }

        public void Dispose()
        {
            if (!IsInit) return;

            SteamAPI.Shutdown();
            IsInit = false;
        }
    }
}
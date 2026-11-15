using Game.Shared.Interface;
using Steamworks;

namespace Game.Platform.Steam
{
    // PlayerId is the SteamID, so approval's duplicate check works across machines and restarts.
    public class SteamIdentityProvider : IPlayerIdentityProvider
    { 
        public string PlayerId { get; }
        public string DisplayName { get; }


        // Takes SteamPlatform only to make the dependency explicit: Steam must be initialized first.
        public SteamIdentityProvider(SteamPlatform platform)
        {
            PlayerId = SteamUser.GetSteamID().m_SteamID.ToString();
            DisplayName = SteamFriends.GetPersonaName();
        }
    }
}
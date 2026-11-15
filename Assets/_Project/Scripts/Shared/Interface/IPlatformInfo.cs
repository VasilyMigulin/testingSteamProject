using UnityEngine;

namespace Game.Shared.Interface
{
    // Lets the UI adapt to the platform without depending on it (address field, invites, notices).
    public interface IPlatformInfo
    {
        bool IsSteam { get;}
        // Shown in the menu, e.g. why Steam is not used. Empty if there is nothing to tell.
        string StartupNotice { get; }
    }
}
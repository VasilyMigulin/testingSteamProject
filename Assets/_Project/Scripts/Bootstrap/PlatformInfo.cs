using Game.Shared.Interface;

namespace Game.Bootstrap
{
    internal sealed class PlatformInfo : IPlatformInfo
    {
        public bool IsSteam { get; }
        public string StartupNotice { get; }

        public PlatformInfo(bool isSteam, string startupNotice)
        {
            IsSteam = isSteam;
            StartupNotice = startupNotice;
        }
    }
}
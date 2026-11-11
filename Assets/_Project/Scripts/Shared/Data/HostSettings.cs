namespace Game.Shared.Data
{
    public readonly struct HostSettings
    {
        public readonly string SessionName;
        public readonly int MaxPlayers;
        public readonly SessionVisibility Visibility;

        public HostSettings(string sessionName, int maxPlayers, SessionVisibility sessionVisibility)
        {
            SessionName = sessionName;
            MaxPlayers = maxPlayers;
            Visibility = sessionVisibility;
        }
    }
}
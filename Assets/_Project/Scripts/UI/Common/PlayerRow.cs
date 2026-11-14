namespace Game.UI.Common
{
    public readonly struct PlayerRow
    {
        public readonly ulong ClientId;
        public readonly string Label;
        public readonly string Ping;
        public readonly bool CanKick;

        public PlayerRow(ulong clientId, string label, string ping, bool canKick)
        {
            ClientId = clientId;
            Label = label;
            Ping = ping;
            CanKick = canKick; 
        } 
    }
}
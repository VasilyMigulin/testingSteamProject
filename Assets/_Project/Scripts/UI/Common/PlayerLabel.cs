using Game.Shared.Data;

namespace Game.UI.Common
{
    internal static class PlayerLabel
    {
        public static string Format(PlayerInfo player)
        {  
            string line = player.Name;
            
            if (player.IsHost)
                line += " (host)"; 
            
            if (player.IsLocal)
                line += " (you)";

            return line;
        }
    }
}
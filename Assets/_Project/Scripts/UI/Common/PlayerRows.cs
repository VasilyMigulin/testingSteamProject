using System.Collections.Generic;
using Game.Shared.Data;

namespace Game.UI.Common
{
    internal static class PlayerRows
    {
        public static List<PlayerRow> Build(IReadOnlyList<PlayerInfo> players, bool canKick)
        {
            var rows = new List<PlayerRow>(players.Count);
            foreach (PlayerInfo player in players)
            {
                rows.Add(new PlayerRow(
                    player.ClientId,
                    PlayerLabel.Format(player),
                    player.IsHost ? "-" : $"{player.PingMs} ms",
                    canKick && !player.IsHost));
            }

            return rows;
        }
    }
}
using Game.Shared.Interface;
using UnityEngine;

namespace Game.Platform.Local
{
    // Stub for non-Steam builds: same events, logged instead of stored.
    public sealed class LogAchievementService : IAchievementService
    { 
        private int _itemsThrown; 
        public void ReportItemThrown() => Debug.Log($"[Stats] items_thrown = {++_itemsThrown}");
    }
}
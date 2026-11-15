using System;
using Game.Shared.Data;
using Game.Shared.Interface;
using Steamworks;
using UnityEngine;
using VContainer.Unity;

namespace Game.Platform.Steam
{
    // Steam stats on placeholder API names (AppID 480 doesn't define them, so Steam calls return false and we log it).
    public sealed class SteamAchievementService : IAchievementService, IStartable, IDisposable
    {
        private const string ItemsThrownStat = "items_thrown";
        private const string ThrowTenAchievement = "ACH_THROW_10";
        private const int ThrowTenThreshold = 10;

        private readonly IConnectionService _connection;
        private int _itemsThrown;
        private bool _hasUnsavedChanges;

        public SteamAchievementService(IConnectionService connection, SteamPlatform platform)
        {
            _connection = connection;
            // Steam loads the local user's stats at initialization, so the current value is available here.
            SteamUserStats.GetStat(ItemsThrownStat, out _itemsThrown);
        } 

        public void Start() => _connection.StateChanged += OnConnectionStateChanged;

        public void ReportItemThrown()
        { 
            _itemsThrown++;

            // SetStat only changes the value locally in the Steam client; StoreStats sends it to Valve.
            if (!SteamUserStats.SetStat(ItemsThrownStat, _itemsThrown)) 
                Debug.Log($"[Stats] {ItemsThrownStat} = {_itemsThrown} (stat is not defined for this AppID)");

            _hasUnsavedChanges = true;

            if (_itemsThrown == ThrowTenThreshold)
            { 
                SteamUserStats.SetAchievement(ThrowTenAchievement);
                // Store right away so the player sees the unlock notification now, not at the end of the session.
                Flush();
            }
        }

        // StoreStats is rate-limited by Steam, so it is called at safe points instead of on every event.
        private void Flush()
        {
            if (!_hasUnsavedChanges) return;
             
            SteamUserStats.StoreStats();
            _hasUnsavedChanges = false;
            Debug.Log("[Stats] StoreStats");    
        }

        private void OnConnectionStateChanged(ConnectionState state)
        {
            if (state == ConnectionState.Offline) Flush();
        }

        public void Dispose()
        { 
            _connection.StateChanged -= OnConnectionStateChanged; 
            Flush(); 
        }
    }
}
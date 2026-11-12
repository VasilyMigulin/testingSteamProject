using Game.Shared.Interface;
using System;
using System.Threading;
using UnityEngine;

namespace Game.Platform.Local
{
    // Identity for non steam build, a guid persisted in PlayerPrefs
    public sealed class GuidIdentityProvider : IPlayerIdentityProvider, IDisposable
    {
        private const string PlayerIdKey = "PlayerId"; 
        private const int MaxSlots = 16;

        // Held for the whole app lifetime, the os releases it if the process dies
        private readonly Mutex _slotMutex;

        public string PlayerId { get; }

        public string DisplayName { get; }

        public GuidIdentityProvider(string displayNameOverride)
        {
            _slotMutex = AcquireSlot(out int slot);

            string key = $"{PlayerIdKey}.{slot}";
            string playerId = PlayerPrefs.GetString(key, string.Empty);

            if (string.IsNullOrEmpty(playerId))
            {
                playerId = Guid.NewGuid().ToString("N");
                PlayerPrefs.SetString(key, playerId);
                PlayerPrefs.Save();
            }

            PlayerId = playerId;
            DisplayName = string.IsNullOrWhiteSpace(displayNameOverride) ? $"Player-{playerId.Substring(0, 4)}" : displayNameOverride;
        }

        // Several instances on one pc share PlayerPrefs. Each running instance take the first free slot
        // so concurrent instances get different ids while a restarted instance get its old id back
        private static Mutex AcquireSlot(out int slot)
        {
            for (slot = 0; slot < MaxSlots; slot++)
            {
                var mutex = new Mutex(true, $"{Application.productName}.PlayerSlot.{slot}", out bool createdNew);
                if (createdNew) return mutex;

                mutex.Dispose();
            }

            throw new InvalidOperationException("No free player slot - too many instances running");
        }

        public void Dispose()
        {
            _slotMutex.ReleaseMutex();
            _slotMutex.Dispose();
        }
    }
}
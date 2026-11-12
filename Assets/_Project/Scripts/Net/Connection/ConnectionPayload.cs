using System;
using System.Text;
using UnityEngine;

namespace Game.Net.Connection
{ 
    // Sent by client in connection request, checked by host while approve 
    [Serializable]
    internal sealed class ConnectionPayload
    {
        public string PlayerId;
        public string PlayerName;
        public string BuildVersion;
        public byte[] ToBytes() => Encoding.UTF8.GetBytes(JsonUtility.ToJson(this));
        public static bool TryParse(byte[] data, out ConnectionPayload payload)
        {
            payload = null;

            if (data == null || data.Length == 0) return false;

            try
            {
                payload = JsonUtility.FromJson<ConnectionPayload>(Encoding.UTF8.GetString(data));

                return payload != null && !string.IsNullOrEmpty(payload.PlayerId);
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
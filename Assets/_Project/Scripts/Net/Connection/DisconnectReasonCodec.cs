using Game.Shared.Data;
using System;
using System.Globalization; 

namespace Game.Net.Connection
{
    // NGO transfer disconnect reason like a strings, we send enum code
    internal static class DisconnectReasonCodec
    {
        public static string Encode(DisconnectReason reason) => ((byte)reason).ToString(CultureInfo.InvariantCulture);
        
        // Unknown or empty code map to none, for example new build sent code we dont know
        public static DisconnectReason Decode(string value)
        {
            if (byte.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out byte code)
                && Enum.IsDefined(typeof(DisconnectReason), code))
            {
                return (DisconnectReason)code;
            }

            return DisconnectReason.None;
        }
    }
}
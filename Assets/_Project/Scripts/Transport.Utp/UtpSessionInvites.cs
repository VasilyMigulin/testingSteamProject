using System.Net;
using System.Net.Sockets;
using Game.Shared.Interface;

namespace Game.Transport.Utp
{ 
    // Direct IP has no invite dialog; the host shares its LAN address instead.
    public sealed class UtpSessionInvites : ISessionInvites
    {
        public bool CanInvite => false;
        public string SessionCode { get; }
        public UtpSessionInvites(ushort port)
        {
            SessionCode = $"{FindLanAddress()}:{port}";
        }

        public void OpenInviteDialog()
        {
        }
        private static string FindLanAddress()
        { 
            foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            { 
                if (address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(address)) return address.ToString();
            }

            return "127.0.0.1";
        }
    }
}
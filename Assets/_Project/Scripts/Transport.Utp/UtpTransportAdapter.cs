using Game.Shared.Data;
using Game.Shared.Interface;
using System;
using System.Globalization;
using System.Net;
using System.Threading;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Game.Transport.Utp
{
    // Direct IP port connection via Unity Transport
    public sealed class UtpTransportAdapter : ITransportAdapter
    {
        public const ushort DefaultPort = 7777;

        private const string ListenAllAddresses = "0.0.0.0";
        private const string LocalAddress = "127.0.0.1";

        private readonly NetworkManager _networkManager;
        private readonly UnityTransport _transport;
        private readonly ushort _port;

        // IP connection have no platform invites so the event is never raised 
        // empty accessors avoid the "event is never used" warning
        public event Action<string> JoinRequested { add { } remove { } }

        public UtpTransportAdapter(NetworkManager networkManager, UnityTransport transport, ushort port)
        {
            _networkManager = networkManager;
            _transport = transport;
            _port = port;
        }

        public Awaitable<ConnectResult> PrepareHostAsync(HostSettings settings, CancellationToken ct)
        {
            UseThisTransport();
            // Listen on all interfaces so other PCs in the LAN can connect.
            _transport.SetConnectionData(LocalAddress, _port, ListenAllAddresses);
            return Completed(ConnectResult.Ok());
        }

        public Awaitable<ConnectResult> PrepareClientAsync(string address, CancellationToken ct)
        {
            if (!TryParseAddress(address, out string ip, out ushort port)) return Completed(ConnectResult.Fail(DisconnectReason.SessionNotFound));

            UseThisTransport();
            _transport.SetConnectionData(ip, port);
            return Completed(ConnectResult.Ok());
        }

        public void Cleanup()
        { 
            // Nothing to release for a direct IP connection.
        }

        private void UseThisTransport() => _networkManager.NetworkConfig.NetworkTransport = _transport;

        // Accepts IP, IP port and local host
        private bool TryParseAddress(string address, out string ip, out ushort port)
        {
            ip = null;
            port = _port;

            if (string.IsNullOrWhiteSpace(address)) return false;

            string host = address.Trim();
            int colon = host.LastIndexOf(':');
            if (colon >= 0)
            {
                if (!ushort.TryParse(host[(colon + 1)..], NumberStyles.None, CultureInfo.InvariantCulture, out port))
                    return false;

                host = host[..colon];
            }

            if (host.Equals("localHost", StringComparison.OrdinalIgnoreCase)) host = LocalAddress;

            if (!IPAddress.TryParse(host, out _))
                return false;

            ip = host;
            return true;
        }

        // Preparation is sync for IP, but the contract is async because steam need it.
        private static Awaitable<ConnectResult> Completed(ConnectResult result)
        {
            var source = new AwaitableCompletionSource<ConnectResult>();
            source.SetResult(result);
            return source.Awaitable;
        }
    }
}
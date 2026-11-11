using System;
using System.Threading;
using Game.Shared.Data;
using UnityEngine;

namespace Game.Shared.Interface
{
    // Entry point for starting and stopping a net session, nothing knows about the concrete transport: ip or steam
    public interface IConnectionService
    {
        bool IsHost { get; }
        ConnectionState State { get; }
        DisconnectReason LastDisconnectReason { get; }
        event Action<ConnectionState> StateChanged;
        Awaitable<ConnectResult> HostAsync(HostSettings settings, CancellationToken cancellationToken);
        // Address format is transport spec (ip or steam)
        // Completes when the host approves the connection or fail
        Awaitable<ConnectResult> JoinAsync(string address, CancellationToken cancellationToken);
        void Leave();
    }
}

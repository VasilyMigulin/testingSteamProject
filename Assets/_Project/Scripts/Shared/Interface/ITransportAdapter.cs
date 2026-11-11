using Game.Shared.Data;
using System;
using System.Threading;
using UnityEngine;

namespace Game.Shared.Interface
{
    // Prepare transport before start NGO
    // utp parses ip, steam create or join a lobby and resolve the steam id host
    public interface ITransportAdapter
    {
        // Raised when platform asks to join a session or steam invite was accept
        event Action<string> JoinRequested;

        Awaitable<ConnectResult> PrepareHostAsync(HostSettings settings, CancellationToken cancellationToken);
        Awaitable<ConnectResult> PrepareClientAsync(string address, CancellationToken cancellationToken);

        // Called when session ends (steam: leave lobby)
        void Cleanup();
    }
}
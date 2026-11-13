using System;
using System.Collections.Generic;
using Game.Shared.Data;

namespace Game.Shared.Interface
{
    // Read-only view of everyone in the current session replicated from the host
    public interface IPlayerRegistry
    {
        IReadOnlyList<PlayerInfo> Players { get; }
        event Action Changed;
    }
}
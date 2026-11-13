using System;
using System.Collections.Generic;
using Game.Shared.Data;
using Game.Shared.Interface;

namespace Game.Net.Session
{
    // Plain service the UI can resolve at any time; SessionState pushes replicated data into it.
    public sealed class PlayerRegistry : IPlayerRegistry
    {
        private IReadOnlyList<PlayerInfo> _players = Array.Empty<PlayerInfo>(); 
        public IReadOnlyList<PlayerInfo> Players => _players; 
        public event Action Changed;

        internal void Set(IReadOnlyList<PlayerInfo> players)
        {
            _players = players;
            Changed?.Invoke();
        }

        internal void Clear() => Set(Array.Empty<PlayerInfo>());
    }
}
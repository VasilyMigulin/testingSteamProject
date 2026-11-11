using UnityEngine;

namespace Game.Shared.Interface
{
    // Player identity - local saved guid or steam id
    public interface IPlayerIdentityProvider
    {
        string PlayerId { get; }
        string DisplayName { get; }
    }
}
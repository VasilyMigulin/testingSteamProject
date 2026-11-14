using System;
using Game.Shared.Data;

namespace Game.Shared.Interface
{
    // What the local player can do right now; gameplay writes it, the HUD shows it.
    public interface IInteractionHint
    {
        InteractionHint Current { get; }
        event Action<InteractionHint> Changed; 
    }
}
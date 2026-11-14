using System;
using Game.Shared.Data;
using Game.Shared.Interface;

namespace Game.Gameplay.Items
{
    public sealed class InteractionHintState : IInteractionHint
    {
        public InteractionHint Current { get; private set; }

        public event Action<InteractionHint> Changed;

        // Called every frame by the interactor, so it only notifies on real changes.
        internal void Set(InteractionHint hint)
        { 
            if (Current == hint) return;
             
            Current = hint;
            Changed?.Invoke(hint);
        }
    }
}
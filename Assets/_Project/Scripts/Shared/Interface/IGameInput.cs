using UnityEngine;

namespace Game.Shared.Interface
{
    // Input abstraction: gameplay and UI depend on intents, not on devices or action assets.
    public interface IGameInput
    {
        Vector2 Move { get; }
        Vector2 Look { get; }
        bool JumpPressed { get; }
        bool PlayerListHeld { get; }
        bool PausePressed { get; }
        bool InteractPressed { get; }    // E: grab / drop
        bool ThrowPressed { get; }       // LMB
        bool SpawnItemPressed { get; }   // F
        bool DeleteItemPressed { get; }  // X
        bool ChangeColorPressed { get; } // C


        // Disables character control and frees the cursor (e.g. while a menu is open).
        void SetGameplayEnabled(bool enabled);
    }
}

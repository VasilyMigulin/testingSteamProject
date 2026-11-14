using System;

namespace Game.Shared.Interface
{
    // Whether the local player is currently loading a network scene.
    public interface ILoadingState
    {
        bool IsLoading { get; }
        event Action<bool> LoadingChanged; 
    }
}
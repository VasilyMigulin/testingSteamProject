namespace Game.Shared.Interface
{
    // Session-level transitions (lobby to game) For host only
    public interface ISessionFlow
    {
        void StartGame();
    }
}
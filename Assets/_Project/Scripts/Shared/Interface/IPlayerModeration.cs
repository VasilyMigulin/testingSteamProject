namespace Game.Shared.Interface
{
    public interface IPlayerModeration
    { 
        // True only on the host.
        bool CanKick { get; }
        void Kick(ulong clientId);
    }
}
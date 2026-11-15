namespace Game.Shared.Interface
{
    // How other players can join the current session.
    public interface ISessionInvites
    { 
        // Platform invite dialog (Steam overlay) is available.
        bool CanInvite { get; }
        // What a friend types into Join: Steam lobby id or ip:port. Empty when there is nothing to share.
        string SessionCode { get; }
        void OpenInviteDialog();
    }
}
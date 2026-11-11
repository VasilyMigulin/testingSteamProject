namespace Game.Shared.Data
{
    // Sent to network, this never reorder and never reuse 
    public enum DisconnectReason : byte
    {
        None = 0,
        SessionFull = 1,
        VersionMismatch = 2,
        DuplicatePlayer = 3,
        Timeout = 4,
        Kicked = 5,
        HostDisconnected = 6, 
        SessionNotFound = 7,
        StartFailed = 8,
    }
}
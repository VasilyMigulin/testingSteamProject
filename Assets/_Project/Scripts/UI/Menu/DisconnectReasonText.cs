using Game.Shared.Data;

namespace Game.UI.Menu
{
    internal static class DisconnectReasonText
    {
        public static string Get(DisconnectReason reason) => reason switch
        {
            DisconnectReason.SessionFull => "Session is full",
            DisconnectReason.VersionMismatch => "Your game version differs from the host",
            DisconnectReason.DuplicatePlayer => "You are already in this session",
            DisconnectReason.Timeout => "Connection timed out",
            DisconnectReason.Kicked => "You were kicked",
            DisconnectReason.HostDisconnected => "Connection to the host was lost",
            DisconnectReason.SessionNotFound => "Session not found",
            DisconnectReason.StartFailed => "Failed to start the session (port in use?)",
            _ => string.Empty,
        };
    }
}
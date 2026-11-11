namespace Game.Shared.Data
{
    public readonly struct ConnectResult
    {
        public readonly bool Success;
        public readonly DisconnectReason FailReason;

        private ConnectResult(bool success, DisconnectReason disconnectReason)
        {
            Success = success;
            FailReason = disconnectReason;
        }

        public static ConnectResult Ok() => new(true, DisconnectReason.None);
        public static ConnectResult Fail(DisconnectReason reason) => new(false, reason); 
    }
}
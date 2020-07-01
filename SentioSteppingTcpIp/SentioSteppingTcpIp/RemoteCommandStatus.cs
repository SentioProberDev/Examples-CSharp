namespace SentioSteppingTcpIp
{
    public enum RemoteCommandStatus : uint
    {
        /// <summary>
        /// No relevant status to report
        /// </summary>
        None = 0,

        /// <summary>
        /// Stepping reached last die
        /// </summary>
        LastDie = 1 << 0,

        /// <summary>
        /// Stepping reached last subsite
        /// </summary>
        LastSite = 1 << 1
    }
}
namespace Msv.AutoMiner.Common.Data.Enums
{
    public enum PoolAvailabilityState : byte
    {
        Available = 0,

        [EnumCaption("No response")]
        NoResponse = 1,

        [EnumCaption("Authentication failed")]
        AuthenticationFailed = 2
    }
}

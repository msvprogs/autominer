using Msv.AutoMiner.Common.Infrastructure;

namespace Msv.AutoMiner.Common.Enums
{
    public enum PoolAvailabilityState : byte
    {
        Available = 0,

        [EnumCaption("No response")]
        NoResponse = 1,

        [EnumCaption("Authentication failed")]
        AuthenticationFailed = 2,

        [EnumCaption("No response (first attempt)")]
        NoResponseAfterFirstAttempt = 3
    }
}

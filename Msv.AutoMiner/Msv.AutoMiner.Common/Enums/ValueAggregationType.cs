using Msv.AutoMiner.Common.Infrastructure;

namespace Msv.AutoMiner.Common.Enums
{
    public enum ValueAggregationType : byte
    {
        [EnumCaption("Most recent")]
        Last = 0,

        [EnumCaption("12 hours average")]
        Last12Hours = 1,

        [EnumCaption("24 hours average")]
        Last24Hours = 2,

        [EnumCaption("3 days average")]
        Last3Days = 3,

        [EnumCaption("1 week average")]
        LastWeek = 4,

        [EnumCaption("2 weeks average")]
        Last2Weeks = 5
    }
}

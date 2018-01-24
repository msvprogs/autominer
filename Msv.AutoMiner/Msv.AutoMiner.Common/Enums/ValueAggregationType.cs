using Msv.AutoMiner.Common.Infrastructure;

namespace Msv.AutoMiner.Common.Enums
{
    public enum ValueAggregationType : byte
    {
        [EnumCaption("Most recent")]
        Last = 0,

        [EnumCaption("12 hours avg")]
        Last12Hours = 1,

        [EnumCaption("24 hours avg")]
        Last24Hours = 2,

        [EnumCaption("3 days avg")]
        Last3Days = 3,

        [EnumCaption("1 week avg")]
        LastWeek = 4,

        [EnumCaption("2 weeks avg")]
        Last2Weeks = 5
    }
}

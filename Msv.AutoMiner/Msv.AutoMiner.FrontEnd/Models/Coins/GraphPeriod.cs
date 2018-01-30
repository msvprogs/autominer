using Msv.AutoMiner.Common.Infrastructure;

namespace Msv.AutoMiner.FrontEnd.Models.Coins
{
    public enum GraphPeriod
    {
        [EnumCaption("Day")]
        Day,

        [EnumCaption("Week")]
        Week,

        [EnumCaption("Two weeks")]
        TwoWeeks,

        [EnumCaption("One month")]
        Month,

        [EnumCaption("Six months")]
        SixMonths
    }
}

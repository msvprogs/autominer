using Msv.AutoMiner.Common.Infrastructure;

namespace Msv.AutoMiner.Common.Enums
{
    public enum MinerApiType : byte
    {
        [EnumCaption("None (parse output)")]
        Stdout = 0,

        [EnumCaption("Claymore Dual Miner")]
        ClaymoreDual = 1
    }
}

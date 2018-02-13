namespace Msv.AutoMiner.Common.Data.Enums
{
    public enum MinerApiType : byte
    {
        [EnumCaption("None (parse output)")]
        Stdout = 0,

        [EnumCaption("Claymore Dual Miner")]
        ClaymoreDual = 1
    }
}

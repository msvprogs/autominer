using Msv.AutoMiner.Common.Data;

namespace Msv.AutoMiner.Data
{
    public enum CoinLastNetworkInfoResult
    {
        Success = 0,

        [EnumCaption("Exception")]
        Exception = 1,

        [EnumCaption("No PoW blocks")]
        NoPoWBlocks = 2,

        [EnumCaption("Out of sync")]
        OutOfSync = 3,

        [EnumCaption("Block reward mismatch")]
        BlockRewardMismatch = 4
    }
}

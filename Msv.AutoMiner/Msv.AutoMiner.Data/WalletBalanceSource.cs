using Msv.AutoMiner.Common.Data;

namespace Msv.AutoMiner.Data
{
    public enum WalletBalanceSource : byte
    {
        [EnumCaption("Local node")]
        LocalNode = 0,

        [EnumCaption("Block explorer")]
        BlockExplorer = 1
    }
}

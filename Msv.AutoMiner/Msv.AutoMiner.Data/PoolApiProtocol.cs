using Msv.AutoMiner.Common.Infrastructure;

namespace Msv.AutoMiner.Data
{
    public enum PoolApiProtocol
    {
        [EnumCaption("None (do not use API)")]
        None = 0,

        [EnumCaption("Qwak - used on the most pools")]
        Qwak = 1,

        [EnumCaption("TheBlockFactory")]
        Tbf = 2,

        [EnumCaption("Open Ethereum")]
        OpenEthereum = 3,

        [EnumCaption("Bitfly")]
        Bitfly = 4,

        [EnumCaption("Node Open Mining Portal")]
        NodeOpenMiningPortal = 6,

        [EnumCaption("Yiimp")]
        Yiimp = 8,

        [EnumCaption("JSON-RPC (local node)")]
        JsonRpcWallet = 9
    }
}

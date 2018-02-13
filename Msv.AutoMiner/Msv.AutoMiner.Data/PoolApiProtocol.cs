using Msv.AutoMiner.Common.Data;

namespace Msv.AutoMiner.Data
{
    public enum PoolApiProtocol
    {
        [EnumCaption("None (do not use API)")]
        None = 0,

        [EnumCaption("Qwak - used on MPOS pools")]
        Qwak = 1,

        [EnumCaption("TheBlockFactory")]
        Tbf = 2,

        [EnumCaption("Open Ethereum Pool")]
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

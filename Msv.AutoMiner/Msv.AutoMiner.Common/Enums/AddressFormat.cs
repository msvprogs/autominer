using Msv.AutoMiner.Common.Infrastructure;

namespace Msv.AutoMiner.Common.Enums
{
    public enum AddressFormat : byte
    {
        [EnumCaption("Base58Check (Bitcoin-like)")]
        Base58Check = 0,

        [EnumCaption("Hex (Ethereum-like)")]
        EthereumHex = 1,

        Special = 2
    }
}

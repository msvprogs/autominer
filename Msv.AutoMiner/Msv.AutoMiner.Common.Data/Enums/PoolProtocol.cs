namespace Msv.AutoMiner.Common.Data.Enums
{
    public enum PoolProtocol : byte
    {
        [EnumCaption("Stratum (public pools)")]
        Stratum = 0,

        [EnumCaption("JSON-RPC (wallet solo-mining)")]
        JsonRpc = 1
    }
}
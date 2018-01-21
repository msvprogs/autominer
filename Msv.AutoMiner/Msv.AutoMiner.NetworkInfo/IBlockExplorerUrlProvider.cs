using System;

namespace Msv.AutoMiner.NetworkInfo
{
    public interface IBlockExplorerUrlProvider
    {
        Uri CreateTransactionUrl(string hash);
        Uri CreateAddressUrl(string address);
        Uri CreateBlockUrl(string blockHash);
    }
}
using System;
using Msv.AutoMiner.NetworkInfo.Data;

namespace Msv.AutoMiner.NetworkInfo.Utilities
{
    public interface IBlockChainSearcher
    {
        BlockHeader SearchBlockWhere(Predicate<BlockHeader> predicate, BlockHeader last);
        BlockHeader SearchPoWBlock(BlockHeader last);
    }
}

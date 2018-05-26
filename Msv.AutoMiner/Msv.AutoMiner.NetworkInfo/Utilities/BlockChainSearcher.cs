using System;
using Msv.AutoMiner.NetworkInfo.Data;

namespace Msv.AutoMiner.NetworkInfo.Utilities
{
    public class BlockChainSearcher : IBlockChainSearcher
    {        
        private const int SearchDepth = 70;

        private readonly Func<string, BlockHeader> m_PreviousBlockGetter;

        public BlockChainSearcher(Func<string, BlockHeader> previousBlockGetter)
            => m_PreviousBlockGetter = previousBlockGetter ?? throw new ArgumentNullException(nameof(previousBlockGetter));

        public BlockHeader SearchBlockWhere(Predicate<BlockHeader> predicate, BlockHeader last)
        {
            if (predicate == null) 
                throw new ArgumentNullException(nameof(predicate));
            if (last == null) 
                throw new ArgumentNullException(nameof(last));

            var currentDepth = 0;
            while (!predicate.Invoke(last)
                   && currentDepth++ <= SearchDepth
                   && last.Height > 0)
                last = m_PreviousBlockGetter.Invoke(last.PreviousBlockHash);
            if (currentDepth > SearchDepth || last.Height == 0)
                throw new NoPoWBlocksException("Couldn't find PoW blocks, searched among last " + currentDepth);
            return last;
        }

        public BlockHeader SearchPoWBlock(BlockHeader last)
            => SearchBlockWhere(
                x => x.Flags == null || x.Flags.ToLowerInvariant().Contains("proof-of-work"),
                last);
    }
}

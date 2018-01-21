using Msv.AutoMiner.Data;
using Msv.AutoMiner.NetworkInfo;

namespace Msv.AutoMiner.FrontEnd.Infrastructure.Contracts
{
    public interface IBlockExplorerUrlProviderFactory
    {
        IBlockExplorerUrlProvider Create(Coin coin);
    }
}

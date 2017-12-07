using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Storage
{
    public class CoinInfoControllerStorage : ICoinInfoControllerStorage
    {
        private readonly AutoMinerDbContext m_Context;

        public CoinInfoControllerStorage(AutoMinerDbContext context)
        {
            m_Context = context;
        }

        public async Task<CoinAlgorithm[]> GetAlgorithms()
            => await m_Context.CoinAlgorithms
                .AsNoTracking()
                .ToArrayAsync();
    }
}

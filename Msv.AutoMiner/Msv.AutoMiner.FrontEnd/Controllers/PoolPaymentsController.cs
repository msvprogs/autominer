using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.FrontEnd.Infrastructure;
using Msv.AutoMiner.FrontEnd.Models.PoolPayments;
using Msv.AutoMiner.FrontEnd.Models.Shared;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class PoolPaymentsController : PaginationController
    {
        private readonly AutoMinerDbContext m_Context;

        public PoolPaymentsController(AutoMinerDbContext context)
        {
            m_Context = context;
        }

        public async Task<IActionResult> Index(int? poolId, Guid? currencyId, int page = 1)
        {
            int[] pools = null;
            var title = "Pool payments ";
            if (poolId != null)
            {
                pools = new[] { poolId.Value };
                title += $"for pool {(await m_Context.Pools.FirstOrDefaultAsync(x => x.Id == poolId))?.Name}";
            }
            else if (currencyId != null)
            {
                pools = m_Context.Pools
                    .Where(x => x.CoinId == currencyId)
                    .Select(x => x.Id)
                    .ToArray();
                title += $"for {(await m_Context.Coins.FirstOrDefaultAsync(x => x.Id == currencyId))?.Name}";
            }

            IQueryable<PoolPayment> payments = m_Context.PoolPayments
                .Include(x => x.Pool)
                .Include(x => x.Pool.Coin)
                .AsNoTracking()
                .OrderByDescending(x => x.DateTime);
            if (pools != null)
                payments = payments.Where(x => pools.Contains(x.PoolId));

            return View(new PaginationModel<PoolPaymentModel>
            {
                CurrentPageItems = await GetCurrentPageItems(payments, page)
                    .Select(x => new PoolPaymentModel
                    {
                        Amount = x.Amount,
                        CurrencySymbol = x.Pool.Coin.Symbol,
                        CurrencyName = x.Pool.Coin.Name,
                        DateTime = x.DateTime,
                        Id = x.ExternalId,
                        PoolName = x.Pool.Name,
                        Transaction = x.Transaction
                    })
                    .ToArrayAsync(),
                CurrentPage = page,
                TotalPages = await CountTotalPages(payments),
                Title = title
            });
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.FrontEnd.Infrastructure;
using Msv.AutoMiner.FrontEnd.Infrastructure.Contracts;
using Msv.AutoMiner.FrontEnd.Models.PoolPayments;
using Msv.AutoMiner.FrontEnd.Models.Shared;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class PoolPaymentsController : PaginationController
    {
        private readonly IBlockExplorerUrlProviderFactory m_BlockExplorerFactory;
        private readonly AutoMinerDbContext m_Context;

        public PoolPaymentsController(IBlockExplorerUrlProviderFactory blockExplorerFactory, AutoMinerDbContext context)
        {
            m_BlockExplorerFactory = blockExplorerFactory;
            m_Context = context;
        }

        public async Task<IActionResult> Index(int? poolId, Guid? currencyId, int page = 1)
        {
            int[] pools = null;
            var title = "Pool payments ";
            if (poolId != null)
            {
                pools = new[] {poolId.Value};
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
                CurrentPageItems = GetCurrentPageItems(payments, page)
                    .AsEnumerable()
                    .Select(x => (item: x, blockExplorer: m_BlockExplorerFactory.Create(x.Pool.Coin)))
                    .Select(x => new PoolPaymentModel
                    {
                        Amount = x.item.Amount
                                 * (x.item.Type == PoolPaymentType.Reward || x.item.Type == PoolPaymentType.Unknown
                                     ? 1
                                     : -1),
                        CurrencySymbol = x.item.Pool.Coin.Symbol,
                        CurrencyName = x.item.Pool.Coin.Name,
                        DateTime = x.item.DateTime,
                        Id = x.item.ExternalId,
                        PoolName = x.item.Pool.Name,
                        Transaction = x.item.Transaction,
                        TransactionUrl = x.blockExplorer.CreateTransactionUrl(x.item.Transaction),
                        Address = x.item.CoinAddress,
                        AddressUrl = x.blockExplorer.CreateAddressUrl(x.item.CoinAddress),
                        BlockHash = x.item.BlockHash,
                        BlockUrl = x.blockExplorer.CreateBlockUrl(x.item.BlockHash),
                        Type = x.item.Type,
                        CurrencyLogo = x.item.Pool.Coin.LogoImageBytes
                    })
                    .ToArray(),
                CurrentPage = page,
                TotalPages = await CountTotalPages(payments),
                Title = title
            });
        }
    }
}

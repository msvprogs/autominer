using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.FrontEnd.Infrastructure;
using Msv.AutoMiner.FrontEnd.Models.Shared;
using Msv.AutoMiner.FrontEnd.Models.WalletOperations;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class WalletOperationsController : PaginationController
    {
        private readonly AutoMinerDbContext m_Context;

        public WalletOperationsController(AutoMinerDbContext context)
        {
            m_Context = context;
        }

        public async Task<IActionResult> Index(
            int? walletId, ExchangeType? exchange, Guid? currencyId, int page = 1)
        {
            int[] wallets = null;
            var title = "Wallet operations ";
            if (walletId != null)
            {
                wallets = new[] { walletId.Value };
                title += $"for wallet {(await m_Context.Wallets.FirstOrDefaultAsync(x => x.Id == walletId))?.Address}";
            }
            else if (exchange != null)
            {
                wallets = m_Context.Wallets
                    .Where(x => x.ExchangeType == exchange)
                    .Select(x => x.Id)
                    .ToArray();
                title += $"for exchange {exchange}";
            }
            else if (currencyId != null)
            {
                wallets = m_Context.Wallets
                    .Where(x => x.CoinId == currencyId)
                    .Select(x => x.Id)
                    .ToArray();
                title += $"for {(await m_Context.Coins.FirstOrDefaultAsync(x => x.Id == currencyId))?.Name}";
            }

            IQueryable<WalletOperation> operations = m_Context.WalletOperations
                .Include(x => x.Wallet)
                .Include(x => x.Wallet.Coin)
                .AsNoTracking()
                .OrderByDescending(x => x.DateTime);
            if (wallets != null)
                operations = operations.Where(x => wallets.Contains(x.WalletId));

            return View(new PaginationModel<WalletOperationModel>
            {
                CurrentPageItems = await GetCurrentPageItems(operations, page)
                    .Select(x => new WalletOperationModel
                    {
                        DateTime = x.DateTime,
                        Id = x.ExternalId,
                        CurrencySymbol = x.Wallet.Coin.Symbol,
                        CurrencyName = x.Wallet.Coin.Name,
                        Amount = x.Amount,
                        Exchange = x.Wallet.ExchangeType,
                        TargetAddress = x.TargetAddress,
                        Transaction = x.Transaction,
                        CurrencyLogo = x.Wallet.Coin.LogoImageBytes
                    })
                    .ToArrayAsync(),
                CurrentPage = page,
                TotalPages = await CountTotalPages(operations),
                Title = title
            });
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.FrontEnd.Infrastructure;
using Msv.AutoMiner.FrontEnd.Infrastructure.Contracts;
using Msv.AutoMiner.FrontEnd.Models.Shared;
using Msv.AutoMiner.FrontEnd.Models.WalletOperations;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class WalletOperationsController : PaginationController
    {
        private readonly IBlockExplorerUrlProviderFactory m_BlockExplorerFactory;
        private readonly AutoMinerDbContext m_Context;

        public WalletOperationsController(IBlockExplorerUrlProviderFactory blockExplorerFactory, AutoMinerDbContext context)
        {
            m_BlockExplorerFactory = blockExplorerFactory;
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
                .Include(x => x.Wallet.Coin.Algorithm)
                .AsNoTracking()
                .OrderByDescending(x => x.DateTime);
            if (wallets != null)
                operations = operations.Where(x => wallets.Contains(x.WalletId));

            return View(new PaginationModel<WalletOperationModel>
            {
                CurrentPageItems = GetCurrentPageItems(operations, page)
                    .AsEnumerable()
                    .Select(x => (item: x, blockExplorer: m_BlockExplorerFactory.Create(x.Wallet.Coin)))
                    .Select(x => new WalletOperationModel
                    {
                        DateTime = x.item.DateTime,
                        Id = x.item.ExternalId,
                        CurrencySymbol = x.item.Wallet.Coin.Symbol,
                        CurrencyName = x.item.Wallet.Coin.Name,
                        Amount = x.item.Amount,
                        Exchange = x.item.Wallet.ExchangeType,
                        TargetAddress = x.item.TargetAddress,
                        TargetAddressUrl = x.blockExplorer.CreateAddressUrl(x.item.TargetAddress),
                        Transaction = x.item.Transaction,
                        TransactionUrl = x.blockExplorer.CreateTransactionUrl(x.item.Transaction),
                        CurrencyLogo = x.item.Wallet.Coin.LogoImageBytes
                    })
                    .ToArray(),
                CurrentPage = page,
                TotalPages = await CountTotalPages(operations),
                Title = title
            });
        }
    }
}

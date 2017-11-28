﻿using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.FrontEnd.Data;

namespace Msv.AutoMiner.FrontEnd.Providers
{
    public class CoinValueProvider : ICoinValueProvider
    {
        private static readonly TimeSpan M_MinDatePeriod = TimeSpan.FromDays(4);

        private readonly AutoMinerDbContext m_Context;

        public CoinValueProvider(AutoMinerDbContext context) 
            => m_Context = context;

        public CoinValue[] GetCurrentCoinValues()
        {
            var minDate = DateTime.UtcNow - M_MinDatePeriod;
            var maxDates = m_Context.ExchangeMarketPrices
                .AsNoTracking()
                .Where(x => x.SourceCoin.Activity != ActivityState.Deleted)
                .Where(x => x.TargetCoin.Symbol == "BTC")
                .Where(x => x.DateTime > minDate)
                .Select(x => new { x.SourceCoinId, x.Exchange, x.DateTime })
                .AsEnumerable()
                .GroupBy(x => new { x.SourceCoinId, x.Exchange })
                .Select(x => x.OrderByDescending(y => y.DateTime).First().DateTime)
                .Distinct()
                .ToArray();

            var btc = m_Context.Coins.First(x => x.Symbol == "BTC");
            return m_Context.ExchangeMarketPrices
                .AsNoTracking()
                .Where(x => x.SourceCoin.Activity != ActivityState.Deleted)
                .Where(x => x.TargetCoin.Symbol == "BTC")
                .Where(x => maxDates.Contains(x.DateTime))
                .AsEnumerable()
                .GroupBy(x => x.SourceCoinId)
                .Select(x => new CoinValue
                {
                    CurrencyId = x.Key,
                    AverageBtcValue = x.Average(y => y.LastPrice),
                    ExchangePrices = x.GroupBy(y => y.Exchange)
                        .Select(y => new CoinExchangePrice
                        {
                            Exchange = y.Key.ToString(),
                            Price = y.OrderByDescending(z => z.DateTime).First().LastPrice
                        })
                        .ToArray(),
                    Updated = x.Max(y => y.DateTime)
                })
                .Concat(new[] {new CoinValue
                {
                    CurrencyId = btc.Id,
                    AverageBtcValue = 1,
                    Updated = DateTime.UtcNow
                }})
                .ToArray();
        }
    }
}

﻿using System;
using System.ComponentModel.DataAnnotations.Schema;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.Data
{
    public class ExchangeMarketPrice
    {
        public Guid SourceCoinId { get; set; }

        public virtual Coin SourceCoin { get; set; }

        public Guid TargetCoinId { get; set; }

        public virtual Coin TargetCoin { get; set; }

        [Column("Exchange")]
        public ExchangeType ExchangeType { get; set; }

        public virtual Exchange Exchange { get; set; }

        public DateTime DateTime { get; set; }

        public bool IsActive { get; set; }

        public double HighestBid { get; set; }

        public double LowestAsk { get; set; }

        public double LastPrice { get; set; }

        public double LastDayVolume { get; set; }

        public double LastDayHigh { get; set; }

        public double LastDayLow { get; set; }

        public double SellFeePercent { get; set; }

        public double BuyFeePercent { get; set; }
    }
}

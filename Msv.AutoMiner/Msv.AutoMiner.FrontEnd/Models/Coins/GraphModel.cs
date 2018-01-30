using System;

namespace Msv.AutoMiner.FrontEnd.Models.Coins
{
    public class GraphModel
    {
        public Guid Id { get; set; }

        public string CoinName { get; set; }

        public GraphType Type { get; set; }

        public GraphPeriod Period { get; set; }

        public DateTime[] Dates { get; set; }

        public double[] Values { get; set; }
    }
}

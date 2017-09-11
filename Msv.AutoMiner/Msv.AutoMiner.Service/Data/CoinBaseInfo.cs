using System;
using Msv.AutoMiner.Commons.Data;

namespace Msv.AutoMiner.Service.Data
{
    public class CoinBaseInfo
    {
        public string Name { get; }
        public string Symbol { get; }
        public CoinAlgorithm Algorithm { get; }

        public CoinBaseInfo(string name, string symbol, CoinAlgorithm algorithm)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            if (string.IsNullOrEmpty(symbol))
                throw new ArgumentException("Value cannot be null or empty.", nameof(symbol));

            Name = name;
            Symbol = symbol;
            Algorithm = algorithm;
        }

        public override string ToString() => $"{Name} ({Symbol}) [{Algorithm}]";
    }
}

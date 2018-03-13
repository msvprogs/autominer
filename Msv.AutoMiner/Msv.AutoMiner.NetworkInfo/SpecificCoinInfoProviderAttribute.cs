using System;

namespace Msv.AutoMiner.NetworkInfo
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SpecificCoinInfoProviderAttribute : Attribute
    {
        public string Symbol { get; }

        public SpecificCoinInfoProviderAttribute(string symbol) 
            => Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
    }
}

using Msv.AutoMiner.Commons.Data;

namespace Msv.AutoMiner.Service.Data
{
    public struct CoinSymbolAlgorithm
    {
        public string Symbol { get; set; }
        public CoinAlgorithm? Algorithm { get; set; }
    }
}

using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common
{
    public class IquidusWithPosDifficultyInfoProvider : IquidusInfoProvider
    {
        public IquidusWithPosDifficultyInfoProvider(IWebClient webClient, string baseUrl) 
            : base(webClient, baseUrl)
        { }

        protected override double GetDifficulty(dynamic difficultyValue)
            => ParsingHelper.ParseDouble(((string)difficultyValue).Split(':')[1]);

        protected override string GetTransactionUrl()
            => "/ext/getlasttxs/0.00000001";
    }
}

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
    }
}
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;

namespace Msv.AutoMiner.NetworkInfo.Common
{
    public class IquidusWithPosDifficultyInfoProvider : IquidusInfoProvider
    {
        public IquidusWithPosDifficultyInfoProvider(
            IWebClient webClient, string baseUrl, NetworkInfoProviderOptions options) 
            : base(webClient, baseUrl, options)
        { }

        protected override double GetDifficulty(dynamic difficultyValue)
            => ParsingHelper.ParseDouble(((string)difficultyValue).Split(':')[1]);
    }
}
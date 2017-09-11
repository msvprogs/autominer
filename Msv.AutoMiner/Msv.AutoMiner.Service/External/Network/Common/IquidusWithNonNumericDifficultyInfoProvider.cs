namespace Msv.AutoMiner.Service.External.Network.Common
{
    public class IquidusWithNonNumericDifficultyInfoProvider : IquidusInfoProvider
    {
        public IquidusWithNonNumericDifficultyInfoProvider(string baseUrl) 
            : base(baseUrl)
        { }

        protected override double GetDifficulty(dynamic difficultyValue)
            => ParsingHelper.ParseDouble(((string)difficultyValue).Split(':')[1]);

        protected override string GetTransactionUrl()
            => "/ext/getlasttxs/0.00000001";
    }
}

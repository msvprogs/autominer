using Msv.AutoMiner.Common.Models.CoinInfoService;

namespace Msv.AutoMiner.Data.Logic.Contracts
{
    public interface IProfitabilityTableBuilder
    {
        SingleProfitabilityData[] Build(ProfitabilityRequest request);
        EstimateProfitabilityResponse EstimateProfitability(EstimateProfitabilityRequest request);
    }
}

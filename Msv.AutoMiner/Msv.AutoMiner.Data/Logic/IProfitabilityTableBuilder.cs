using Msv.AutoMiner.Common.Models.CoinInfoService;

namespace Msv.AutoMiner.Data.Logic
{
    public interface IProfitabilityTableBuilder
    {
        SingleProfitabilityData[] Build(ProfitabilityRequest request);
        EstimateProfitabilityResponse EstimateProfitability(EstimateProfitabilityRequest request);
    }
}

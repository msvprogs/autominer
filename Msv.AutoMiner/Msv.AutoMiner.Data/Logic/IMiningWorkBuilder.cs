using Msv.AutoMiner.Common.Models.CoinInfoService;
using Msv.AutoMiner.Common.Models.ControlCenterService;

namespace Msv.AutoMiner.Data.Logic
{
    public interface IMiningWorkBuilder
    {
        MiningWorkModel[] Build(SingleProfitabilityData[] profitabilities);
    }
}

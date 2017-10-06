using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.FrontEnd.Models.Exchanges
{
    public class ExchangeModel
    {
        public ExchangeType Type { get; set; }

        public ActivityState Activity { get; set; }

        public bool HasKeys { get; set; }
    }
}

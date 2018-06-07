using System.Linq;
using Msv.AutoMiner.Common.Notifiers;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;
using Msv.AutoMiner.Data.Logic.Contracts;

namespace Msv.AutoMiner.ControlCenterService.Logic.Storage
{
    public class TelegramNotifierStorage : ITelegramNotifierStorage
    {
        private readonly IAutoMinerDbContextFactory m_Factory;

        public TelegramNotifierStorage(IAutoMinerDbContextFactory factory)
            => m_Factory = factory;

        public Rig GetRig(int rigId)
        {
            using (var context = m_Factory.CreateReadOnly())
                return context.Rigs.First(x => x.Id == rigId);
        }

        public int[] GetReceiverIds(string[] userWhiteList)
        {
            using (var context = m_Factory.CreateReadOnly())
                return context.TelegramUsers
                    .Where(x => userWhiteList.Contains(x.UserName))
                    .Select(x => x.Id)
                    .ToArray();
        }
    }
}

using System.Linq;
using Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;

namespace Msv.AutoMiner.ControlCenterService.Logic.Storage
{
    public class NotifierStorage : INotifierStorage
    {
        private readonly IAutoMinerDbContextFactory m_Factory;

        public NotifierStorage(IAutoMinerDbContextFactory factory)
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

using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Logic.Storage
{
    public class RigStatusNotifierStorage : IRigStatusNotifierStorage
    {
        private readonly string m_ConnectionString;

        public RigStatusNotifierStorage(string connectionString)
        {
            m_ConnectionString = connectionString;
        }

        public Rig GetRig(int rigId)
        {
            using (var context = new AutoMinerDbContext(m_ConnectionString))
                return context.Rigs.AsNoTracking().First(x => x.Id == rigId);
        }

        public int[] GetReceiverIds()
        {
            using (var context = new AutoMinerDbContext(m_ConnectionString))
                return context.TelegramUsers.Select(x => x.Id).ToArray();
        }
    }
}

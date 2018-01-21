using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.ControlCenterService.Storage.Contracts;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;

namespace Msv.AutoMiner.ControlCenterService.Storage
{
    public class CertificateServiceStorage : ICertificateServiceStorage
    {
        private readonly IAutoMinerDbContextFactory m_Factory;

        public CertificateServiceStorage(IAutoMinerDbContextFactory factory)
            => m_Factory = factory;

        public Task<Rig> GetRigByName(string name)
        {
            using (var context = m_Factory.Create())
                return context.Rigs.FirstOrDefaultAsync(x => x.Name == name);
        }
    }
}

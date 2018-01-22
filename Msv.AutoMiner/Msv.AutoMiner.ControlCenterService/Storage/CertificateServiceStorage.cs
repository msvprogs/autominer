using System.Linq;
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

        public Rig GetRigByName(string name)
        {
            using (var context = m_Factory.CreateReadOnly())
                return context.Rigs.FirstOrDefault(x => x.Name == name);
        }
    }
}

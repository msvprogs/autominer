using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.ControlCenterService.Storage.Contracts;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Storage
{
    public class CertificateServiceStorage : ICertificateServiceStorage
    {
        private readonly AutoMinerDbContext m_Context;

        public CertificateServiceStorage(AutoMinerDbContext context)
            => m_Context = context;

        public Task<Rig> GetRigByName(string name)
            => m_Context.Rigs.FirstOrDefaultAsync(x => x.Name == name);
    }
}

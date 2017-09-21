using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Security.Contracts
{
    public interface ICertificateService
    {
        Task<X509Certificate2> CreateCertificate(Rig rig, byte[] certificateRequest);
        Task<Rig> AuthenticateRig(X509Certificate2 clientCertificate);
    }
}
using System.Security.Cryptography.X509Certificates;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Security.Contracts
{
    public interface ICertificateService
    {
        X509Certificate2 CreateCertificate(Rig rig, X509Certificate2 serverCertificate, byte[] certificateRequest);
        Rig AuthenticateRig(X509Certificate2 serverCertificate, X509Certificate2 clientCertificate);
    }
}
using System;
using System.Security.Cryptography.X509Certificates;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.Rig.Security;
using NLog;

namespace Msv.AutoMiner.Rig.Remote
{
    public class ControlCenterRegistrator
    {
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        private readonly IClientCertificateProvider m_CertificateProvider;
        private readonly IControlCenterService m_ControlCenterService;

        public ControlCenterRegistrator(
            IClientCertificateProvider certificateProvider, IControlCenterService controlCenterService)
        {
            m_CertificateProvider = certificateProvider ?? throw new ArgumentNullException(nameof(certificateProvider));
            m_ControlCenterService = controlCenterService ?? throw new ArgumentNullException(nameof(controlCenterService));
        }

        public void Register(string name, string password)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            M_Logger.Info($"Creating certification request for {name}...");
            var request = m_CertificateProvider.CreateNewKeys(name);
            M_Logger.Info("Sending certification request to server...");
            var result = m_ControlCenterService.RegisterRig(new RegisterRigRequestModel
            {
                Name = name,
                Password = password,
                X509CertificateRequest = request.CertificationRequest.GetEncoded()
            });
            if (!result.IsSuccess)
            {
                M_Logger.Error("Registration failed. Please check service URL, rig name and registration password");
                return;
            }
            var certificate = new X509Certificate2(result.X509ClientCertificate);
            M_Logger.Info($"Received client certificate (thumbprint {certificate.Thumbprint}), storing it in the local storage...");
            m_CertificateProvider.StoreCaCertificate(new X509Certificate2(result.CaCertificate));
            m_CertificateProvider.StoreClientCertificate(certificate, request.KeyPair);
            M_Logger.Info($"Rig {name} was registered successfully");
        }
    }
}

namespace Msv.AutoMiner.ServerInitializer.Configuration
{
    public class InitializationElement
    {
        public bool CreateDatabase { get; set; }
        public CertificateElement RootCertificate { get; set; }
        public CertificateElement ControlCenterCertificate { get; set; }
        public CertificateElement FrontEndCertificate { get;set; }
    }
}

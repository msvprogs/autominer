namespace Msv.AutoMiner.Common.Models.ControlCenterService
{
    public class RegisterRigRequestModel : LicensedRequestBase
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public byte[] X509CertificateRequest { get; set; }
    }
}
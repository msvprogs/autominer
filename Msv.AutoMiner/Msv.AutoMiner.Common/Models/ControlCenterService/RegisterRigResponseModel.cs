namespace Msv.AutoMiner.Common.Models.ControlCenterService
{
    public class RegisterRigResponseModel
    {
        public bool IsSuccess { get; set; }
        public byte[] X509ClientCertificate { get; set; }
        public byte[] CaCertificate { get; set; }
    }
}

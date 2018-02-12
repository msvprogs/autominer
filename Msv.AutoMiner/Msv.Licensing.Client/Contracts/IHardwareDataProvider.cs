using Msv.Licensing.Client.Data;

namespace Msv.Licensing.Client.Contracts
{
    internal interface IHardwareDataProvider
    {
        HardwareData GetHardwareData();
    }
}

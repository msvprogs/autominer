using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.FrontEnd.Infrastructure.Contracts
{
    public interface IWalletAddressValidatorFactory
    {
        IWalletAddressValidator Create(AddressFormat format, string[] prefixes);
    }
}

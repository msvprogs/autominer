using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.FrontEnd.Infrastructure.Contracts
{
    public interface IWalletAddressValidatorFactory
    {
        IWalletAddressValidator Create(AddressFormat format, string[] prefixes);
    }
}

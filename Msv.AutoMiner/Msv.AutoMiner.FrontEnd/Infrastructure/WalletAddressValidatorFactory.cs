using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.FrontEnd.Infrastructure.Contracts;

namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    public class WalletAddressValidatorFactory : IWalletAddressValidatorFactory
    {
        public IWalletAddressValidator Create(AddressFormat format, string[] prefixes)
        {
            switch (format)
            {
                case AddressFormat.Base58Check:
                    return new Base58CheckWalletAddressValidator(prefixes);
                case AddressFormat.EthereumHex:
                    return new EthereumWalletAddressValidator();
                default:
                    return new DummyValidator();
            }
        }

        private class DummyValidator : IWalletAddressValidator
        {
            public bool HasCheckSum(string address)
                => true;

            public bool IsValid(string address)
                => true;
        }
    }
}

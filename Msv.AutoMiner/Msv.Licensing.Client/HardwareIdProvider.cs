using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Msv.Licensing.Client.Contracts;

namespace Msv.Licensing.Client
{
    internal class HardwareIdProvider : IHardwareIdProvider
    {
        private readonly IHardwareDataProviderFactory m_ProviderFactory;

        public HardwareIdProvider(IHardwareDataProviderFactory providerFactory) 
            => m_ProviderFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));

        [Obfuscation(Exclude = true)]
        public dynamic GetHardwareId()
        {
            var hardwareData = m_ProviderFactory.Create().GetHardwareData();
            var concatenatedHardwareData =
                string.Concat(
                    hardwareData.MotherboardId,
                    hardwareData.MotherboardProductName,
                    hardwareData.ProcessorId,
                    hardwareData.ProcessorSignature,
                    string.Concat(hardwareData.MemoryIds));
            if (string.IsNullOrWhiteSpace(concatenatedHardwareData))
                throw new PlatformNotSupportedException("Couldn't retreive any hardware IDs for licensing");

            using (dynamic sha256 = new SHA256CryptoServiceProvider())
            {
                var hash = sha256.ComputeHash(
                    ((IEnumerable<byte>)sha256.ComputeHash(Encoding.UTF8.GetBytes(concatenatedHardwareData)))
                        .Concat(Encoding.UTF8.GetBytes(
                            "34rthjq5ugwuivn vb tmjh6o27ubb8j56 vofvn5 43[vnhnjvhvbgda=zz=xckv hnb"))
                        .ToArray());
                return Base36.EncodeDelimited(hash, 5);
            }
        }
    }
}

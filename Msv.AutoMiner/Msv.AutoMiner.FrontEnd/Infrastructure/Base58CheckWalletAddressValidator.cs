using System;
using System.Linq;
using System.Security.Cryptography;
using JetBrains.Annotations;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.FrontEnd.Infrastructure.Contracts;

namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    public class Base58CheckWalletAddressValidator : IWalletAddressValidator
    {        
        private const int CheckSumLength = 4;

        private readonly byte[] m_NumberPrefixes;
        private readonly string[] m_StringPrefixes;

        public Base58CheckWalletAddressValidator([NotNull] string[] prefixes)
        {
            if (prefixes == null) 
                throw new ArgumentNullException(nameof(prefixes));

            var numberPrefixes = prefixes
                .Where(HexHelper.IsHexWithPrefix)
                .ToArray();

            m_NumberPrefixes = numberPrefixes
                .Select(x => HexHelper.FromHex(x).First())
                .ToArray();
            m_StringPrefixes = prefixes.Except(numberPrefixes)
                .ToArray();
        }

        public bool HasCheckSum(string address)
            => true;

        public bool IsValid(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return false;
            if (!Base58.IsValidString(address))
                return false;
            
            var addressBytes = Base58.Decode(address);
            using (var sha256 = new SHA256CryptoServiceProvider())
            {
                var dataLength = addressBytes.Length - CheckSumLength;
                var checkSum = sha256.ComputeHash(sha256.ComputeHash(addressBytes, 0, dataLength));
                if (!addressBytes.Skip(dataLength).SequenceEqual(checkSum.Take(CheckSumLength)))
                    return false;
            }

            if (!m_NumberPrefixes.Any() && !m_StringPrefixes.Any())
                return true;

            var isValid = false;
            if (m_NumberPrefixes.Any())
                isValid = m_NumberPrefixes.Contains(addressBytes[0]);
            if (m_StringPrefixes.Any())
                isValid |= m_StringPrefixes.Any(address.StartsWith);
            return isValid;
        }
    }
}

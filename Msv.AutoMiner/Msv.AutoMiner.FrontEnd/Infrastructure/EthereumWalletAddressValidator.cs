using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.FrontEnd.Infrastructure.Contracts;
using Org.BouncyCastle.Crypto.Digests;

namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    public class EthereumWalletAddressValidator : IWalletAddressValidator
    {
        private const int Sha3Bits = 256;

        private static readonly Regex M_AddressRegex = new Regex(
            "^(0x)?[0-9a-f]{40}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public bool HasCheckSum(string address)
            => IsValidFormat(address) && address != address.ToLowerInvariant();

        public bool IsValid(string address)
        {
            if (!IsValidFormat(address))
                return false;

            address = HexHelper.CutPrefix(address);
            return HexHelper.ToHex(CalculateKeccak(address.ToLowerInvariant()))
                .Take(40)
                .Select((x, i) => (index: i, code: byte.Parse(x.ToString(), NumberStyles.HexNumber)))
                .All(x => char.IsNumber(address, x.index)
                          || x.code > 7 && char.IsUpper(address, x.index)
                          || x.code <= 7 && char.IsLower(address, x.index));
        }

        private static bool IsValidFormat(string address)
            => !string.IsNullOrEmpty(address)
               && M_AddressRegex.IsMatch(address);

        private static byte[] CalculateKeccak(string str)
        {
            var digest = new KeccakDigest(Sha3Bits);
            var addressBytes = Encoding.ASCII.GetBytes(str);
            digest.BlockUpdate(addressBytes, 0, addressBytes.Length);
            var hash = new byte[digest.GetDigestSize()];
            digest.DoFinal(hash, 0);
            return hash;
        }
    }
}
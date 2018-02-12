using System.Security.Cryptography;
using Msv.Licensing.Client.Contracts;

namespace Msv.Licensing.Client
{
    internal class LicenseIdGenerator : ILicenseIdGenerator
    {
        private const int Groups = 4;

        public string Generate()
        {
            using (var prng = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[sizeof(uint) * Groups];
                prng.GetBytes(bytes);
                return Base36.EncodeDelimited(bytes, Groups);
            }
        }
    }
}

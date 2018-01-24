using System.Security.Cryptography;
using Msv.AutoMiner.FrontEnd.Infrastructure.Contracts;

namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    public class CryptoRandomGenerator : ICryptoRandomGenerator
    {
        public byte[] GenerateRandom(int bytes)
        {
            using (var prng = new RNGCryptoServiceProvider())
            {
                var byteArray = new byte[bytes];
                prng.GetBytes(byteArray);
                return byteArray;
            }
        }
    }
}

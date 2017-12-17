using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    public class Sha256PasswordHasher : IPasswordHasher
    {
        public byte[] CalculateHash(string password, byte[] salt)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));
            if (salt == null)
                throw new ArgumentNullException(nameof(salt));

            using (var sha256 = new SHA256CryptoServiceProvider())
                return sha256.ComputeHash(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)).Concat(salt).ToArray());
        }
    }
}

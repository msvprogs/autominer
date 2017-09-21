using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using Msv.AutoMiner.ControlCenterService.Security.Contracts;

namespace Msv.AutoMiner.ControlCenterService.Security
{
    public class StringEncryptor : IStringEncryptor
    {
        private const int KeySize = 256;
        private const int BlockSize = 128;

        public byte[] Encrypt(string str)
        {
            var key = GetEncryptionKey();
            var bytes = Encoding.Unicode.GetBytes(str);
            using (var aes = new RijndaelManaged { KeySize = KeySize, BlockSize = BlockSize, Mode = CipherMode.CBC })
            using (var encryptor = aes.CreateEncryptor(key.key, key.iv))
                return encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
        }

        public string Decrypt(byte[] encrypted)
        {
            var key = GetEncryptionKey();
            using (var aes = new RijndaelManaged { KeySize = KeySize, BlockSize = BlockSize, Mode = CipherMode.CBC })
            using (var decryptor = aes.CreateDecryptor(key.key, key.iv))
                return Encoding.Unicode.GetString(
                    decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length));
        }

        private static (byte[] key, byte[] iv) GetEncryptionKey()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var password = identity.Name + "2o45lg263v2tw4g";
                using (var deriver = new Rfc2898DeriveBytes(
                    password, Encoding.UTF8.GetBytes("exchangekeys")))
                    return (deriver.GetBytes(KeySize / 8), deriver.GetBytes(BlockSize / 8));
            }
        }
    }
}

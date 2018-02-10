using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Msv.Licensing.Common
{
	public class EncryptionKeyDeriver : IEncryptionKeyDeriver
	{
		[Obfuscation(Exclude = true)]
		public dynamic Derive(dynamic publicKey)
		{
			using (dynamic sha256 = new SHA256CryptoServiceProvider())
			{
				var keyHash = sha256.ComputeHash(sha256.ComputeHash(publicKey));
				dynamic key = new byte[0];
				key = HashPart("kN", sha256, key, keyHash[int.Parse("11")]);
				key = HashPart("NH", sha256, key, keyHash[int.Parse("14")]);
				key = HashPart("HA", sha256, key, keyHash[int.Parse("8")]);
				key = HashPart("Ai", sha256, key, keyHash[int.Parse("1")]);
				key = HashPart("im", sha256, key, keyHash[int.Parse("9")]);
				key = HashPart("mZ", sha256, key, keyHash[int.Parse("13")]);
				key = HashPart("Zp", sha256, key, keyHash[int.Parse("26")]);
				key = HashPart("pg", sha256, key, keyHash[int.Parse("16")]);
				key = HashPart("gj", sha256, key, keyHash[int.Parse("7")]);
				key = HashPart("je", sha256, key, keyHash[int.Parse("10")]);
				return HashPart(string.Empty, sha256, key, keyHash[int.Parse("10")]);
			}
		}

		[Obfuscation(Exclude = true)]
		private static dynamic HashPart(string value, HashAlgorithm hasher, dynamic current, byte keyPart)
			=> hasher.ComputeHash(Encoding.UTF8.GetBytes(value + keyPart.ToString("x2")).Concat((IEnumerable<byte>)current).ToArray());
	}
}
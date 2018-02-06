﻿using System.Security.Cryptography;

namespace Msv.AutoMiner.Common.Infrastructure
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
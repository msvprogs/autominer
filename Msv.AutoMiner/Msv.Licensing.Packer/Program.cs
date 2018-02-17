using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Msv.Licensing.Common;

namespace Msv.Licensing.Packer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: Packer.exe <application_name> <application_folder>");
                return;
            }

            var appName = args[0];
            var mainFolder = new DirectoryInfo(args[1]);
            Console.WriteLine($"Packing '{appName}' from {mainFolder.FullName}...");

            var filesToPack = mainFolder.GetFiles("Msv.*.dll")
                .Concat(mainFolder.GetFiles("Msv.*.exe"))
                .Except(mainFolder.GetFiles("Msv.Licensing.*"), new FileInfoEqualityComparer())
                .ToArray();

            byte[] publicKey = new PlainPublicKeyProvider().Provide();
            using (var sha1 = new SHA1CryptoServiceProvider())
                Console.WriteLine($"Using public key with SHA-1 hash {BitConverter.ToString(sha1.ComputeHash(publicKey))}");
            byte[] encryptionKey = new EncryptionKeyDeriver().Derive(publicKey);
            byte[] iv;
            using (var sha256 = new SHA256CryptoServiceProvider())
                iv = sha256.ComputeHash(sha256.ComputeHash(encryptionKey));

            var targetFilePath = Path.Combine(mainFolder.FullName, $"{appName}.msvenc");
            using (var targetFileStream =
                new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var aes = new RijndaelManaged
                {
                    Key = encryptionKey,
                    IV = iv.Take(16).ToArray(),
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.Zeros
                })
                using (var encryptor = aes.CreateEncryptor())
                using (var cryptoStream = new CryptoStream(targetFileStream, encryptor, CryptoStreamMode.Write))
                using (var compressedStream = new GZipStream(cryptoStream, CompressionMode.Compress, true))
                {
                    var appNameBytes = Encoding.UTF8.GetBytes(appName);
                    compressedStream.Write(BitConverter.GetBytes(appNameBytes.Length), 0, sizeof(int));
                    compressedStream.Write(appNameBytes, 0, appNameBytes.Length);
                    foreach (var fileInfo in filesToPack)
                    {
                        Console.WriteLine($"Processing file {fileInfo.Name}...");
                        using (var sourceFileStream = fileInfo.OpenRead())
                        {
                            compressedStream.Write(BitConverter.GetBytes((int)fileInfo.Length), 0, sizeof(int));
                            sourceFileStream.CopyTo(compressedStream);
                        }
                    }
                    compressedStream.Flush();
                }
                targetFileStream.Flush();
                Console.WriteLine($"Packing finished. {filesToPack.Length} files packed, target file: {targetFilePath}, size {targetFileStream.Length:N0} bytes");
            }
        }

        private class FileInfoEqualityComparer : EqualityComparer<FileInfo>
        {
            public override bool Equals(FileInfo x, FileInfo y) 
                => x?.FullName == y?.FullName;

            public override int GetHashCode(FileInfo obj)
                => obj?.FullName.GetHashCode() ?? 0;
        }
    }
}

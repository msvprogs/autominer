using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Msv.Licensing.Common;

namespace Msv.Licensing.Client
{
    public class LicensedApplicationLoader : ILicensedApplicationLoader
    {
        private readonly IAssemblyLoader m_AssemblyLoader;
        private readonly string[] m_Args;

        public LicensedApplicationLoader(IAssemblyLoader assemblyLoader, string[] args)
        {
            m_AssemblyLoader = assemblyLoader ?? throw new ArgumentNullException(nameof(assemblyLoader));
            m_Args = args ?? throw new ArgumentNullException(nameof(args));
        }

        public ApplicationLoadResult Load(string applicationName, string licenseFileName)
        {
            if (applicationName == null) 
                throw new ArgumentNullException(nameof(applicationName));
            if (licenseFileName == null) 
                throw new ArgumentNullException(nameof(licenseFileName));

            dynamic licenseFile = new FileInfo(licenseFileName);
            if (!licenseFile.Exists)
                return new ApplicationLoadResult(ApplicationLoadStatus.LicenseNotFound, null);
            var appFiles = licenseFile.Directory.GetFiles($"{applicationName}.msvenc");
            if (appFiles.Length == 0)
                return new ApplicationLoadResult(ApplicationLoadStatus.ApplicationNotFound, null);

            try
            {
                dynamic verifier = new LicenseVerifier(new EncryptionKeyDeriver(), new PublicKeyProvider());
                dynamic assembliesCode = new List<MemoryStream>();
                var decryptionKey = verifier.VerifyAndDerive(applicationName, licenseFileName);
                dynamic iv;
                using (dynamic sha256 = new SHA256CryptoServiceProvider())
                    iv = sha256.ComputeHash(sha256.ComputeHash((byte[]) decryptionKey));

                using (dynamic aes = new RijndaelManaged
                {
                    Key = (byte[]) decryptionKey,
                    IV = ((IEnumerable<byte>)iv).Take(16).ToArray(),
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.Zeros
                })
                using (var decryptor = aes.CreateDecryptor())
                using (var fileStream = appFiles[0].OpenRead())
                using (dynamic decryptingStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read))
                using (dynamic decompressStream = new GZipStream(decryptingStream, CompressionMode.Decompress))
                {
                    dynamic lengthBuffer = new byte[sizeof(int)];

                    decompressStream.Read(lengthBuffer, 0, lengthBuffer.Length);
                    var appNameLength = BitConverter.ToInt32(lengthBuffer, 0);
                    dynamic appNameBuffer = new byte[appNameLength];
                    decompressStream.Read(appNameBuffer, 0, appNameBuffer.Length);
                    if (Encoding.UTF8.GetString(appNameBuffer) != applicationName)
                        return new ApplicationLoadResult(ApplicationLoadStatus.LicenseIsForOtherApplication, null);

                    while (fileStream.Position < fileStream.Length)
                    {
                        decompressStream.Read(lengthBuffer, 0, lengthBuffer.Length);
                        var length = BitConverter.ToInt32(lengthBuffer, 0);
                        dynamic buffer = new byte[length];
                        decompressStream.Read(buffer, 0, length);
                        assembliesCode.Add(new MemoryStream(buffer));
                    }
                }

                var assemblies = m_AssemblyLoader.Load(assembliesCode.ToArray());
                foreach (var stream in assembliesCode)
                    stream.Dispose();

                using (PerVal(applicationName, licenseFileName))
                using (m_AssemblyLoader.CreateResolver(assemblies))
                    ((IEnumerable<dynamic>)assemblies)
                        .Single(x => x.EntryPoint != null)
                        .EntryPoint.Invoke(null, new object[] {m_Args});

                return new ApplicationLoadResult(ApplicationLoadStatus.Success, null);
            }
            catch (LicenseCorruptException)
            {
                return new ApplicationLoadResult(ApplicationLoadStatus.LicenseCorrupt, null);
            }
            catch (LicenseExpiredException)
            {
                return new ApplicationLoadResult(ApplicationLoadStatus.LicenseExpired, null);
            }
            catch (LicenseIsForDifferentApplicationException)
            {
                return new ApplicationLoadResult(ApplicationLoadStatus.LicenseIsForOtherApplication, null);
            }
            catch (CryptographicException cex)
            {
                return new ApplicationLoadResult(ApplicationLoadStatus.LicenseCorrupt, cex);
            }
            catch (Exception ex)
            {
                return new ApplicationLoadResult(ApplicationLoadStatus.UnknownError, ex);
            }
        }

        [Obfuscation(Exclude = true)]
        private dynamic PerVal(dynamic applicationName, dynamic licenseFileName)
        {
            dynamic random = new Random(GetType().GetHashCode());
            dynamic standardCheckInterval = TimeSpan.FromSeconds(double.Parse("10800")); //3 hours
            dynamic maxIntervalDispersion = int.Parse("3600"); //1 hour
            return Observable.Generate(Unit.Default, x => true, x => x, x => x,
                    x => (TimeSpan) (standardCheckInterval
                                     + TimeSpan.FromSeconds(random.Next(-maxIntervalDispersion, maxIntervalDispersion))),
                    TaskPoolScheduler.Default)
                .Subscribe(x =>
                {
                    try
                    {
                        new LicenseVerifier(null, new PublicKeyProvider()).Verify(applicationName, licenseFileName);
                    }
                    catch
                    {
                        // License expired or corrupted - exiting with Environment.Exit(1)
                        dynamic exitMethod = typeof(Environment).GetMethod(nameof(Environment.Exit),
                            BindingFlags.Public | BindingFlags.Static);
                        exitMethod.Invoke(null, new object[] {int.Parse("1")});
                    }
                });
        }
    }
}

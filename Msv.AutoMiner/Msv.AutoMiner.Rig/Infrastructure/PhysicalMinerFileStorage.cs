extern alias oldIoCompression;

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Mono.Unix;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Rig.Infrastructure.Contracts;

namespace Msv.AutoMiner.Rig.Infrastructure
{
    public class PhysicalMinerFileStorage : IMinerFileStorage
    {
        private readonly string m_RootPath;

        public PhysicalMinerFileStorage(string rootPath)
        {
            m_RootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
            if (!Directory.Exists(m_RootPath))
                Directory.CreateDirectory(m_RootPath);
        }

        public string Save(Stream zipStream, string name, int versionId, string mainExecutable)
        {
            if (zipStream == null)
                throw new ArgumentNullException(nameof(zipStream));
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            // OMG, but without external aliases there will be a conflict between different versions of System.IO.Compression library
            using (var zipArchive = new oldIoCompression::System.IO.Compression.ZipArchive(
                zipStream, oldIoCompression::System.IO.Compression.ZipArchiveMode.Read))
            {
                var directoryName = Path.Combine(m_RootPath, $"Miner_{versionId}_{name}".ToSafeFileName());
                if (Directory.Exists(directoryName))
                    Directory.Delete(directoryName, true);
                Directory.CreateDirectory(directoryName);
                zipArchive.ExtractToDirectory(directoryName);

                if (mainExecutable == null || Environment.OSVersion.Platform != PlatformID.Unix)
                    return directoryName;
                // Unix requires to set execute permission on the main executable
                var executableInfo = new UnixFileInfo(Path.Combine(directoryName, mainExecutable));
                executableInfo.FileAccessPermissions |= 
                    FileAccessPermissions.UserExecute | FileAccessPermissions.GroupExecute | FileAccessPermissions.OtherExecute;
                return directoryName;
            }
        }

        public string GetPath(int versionId)
            => new DirectoryInfo(m_RootPath)
                .GetDirectories($"Miner_{versionId}_*")
                .Select(x => x.FullName)
                .FirstOrDefault();

        public void Delete(int versionId)
            => new DirectoryInfo(m_RootPath)
                .GetDirectories($"Miner_{versionId}_*")
                .ToArray()
                .ForEach(x => x.Delete(true));
    }
}

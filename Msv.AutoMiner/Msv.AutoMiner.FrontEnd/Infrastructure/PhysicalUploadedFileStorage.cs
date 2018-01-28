using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Msv.AutoMiner.FrontEnd.Infrastructure.Contracts;

namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    public class PhysicalUploadedFileStorage : IUploadedFileStorage
    {
        private readonly string m_RootPath;

        public PhysicalUploadedFileStorage(string rootPath)
        {
            m_RootPath = rootPath;

            if (!Directory.Exists(rootPath))
                Directory.CreateDirectory(rootPath);
        }

        public async Task SaveAsync([NotNull] string name, [NotNull] Stream contentsStream)
        {
            if (name == null) 
                throw new ArgumentNullException(nameof(name));
            if (contentsStream == null) 
                throw new ArgumentNullException(nameof(contentsStream));

            using (var fileStream = new FileStream(GetFullPath(name), FileMode.Create, FileAccess.Write, FileShare.None))
                await contentsStream.CopyToAsync(fileStream);
        }

        public Stream Load([NotNull] string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return new FileStream(GetFullPath(name), FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public bool Exists([NotNull] string name)
        {
            if (name == null) 
                throw new ArgumentNullException(nameof(name));

            return File.Exists(GetFullPath(name));
        }

        private string GetFullPath(string name)
            => Path.Combine(m_RootPath, name);
    }
}

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Msv.AutoMiner.Common.Infrastructure
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

        public async Task SaveAsync(string name, Stream contentsStream)
        {
            if (name == null) 
                throw new ArgumentNullException(nameof(name));
            if (contentsStream == null) 
                throw new ArgumentNullException(nameof(contentsStream));

            using (var fileStream = new FileStream(GetFullPath(name), FileMode.Create, FileAccess.Write, FileShare.None))
                await contentsStream.CopyToAsync(fileStream);
        }

        public Stream Load(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return new FileStream(GetFullPath(name), FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public string[] Search(string pattern)
        {
            if (pattern == null) 
                throw new ArgumentNullException(nameof(pattern));

            return new DirectoryInfo(m_RootPath)
                .GetFiles(pattern, SearchOption.TopDirectoryOnly)
                .Select(x => x.Name)
                .ToArray();
        }

        private string GetFullPath(string name)
            => Path.Combine(m_RootPath, name);
    }
}

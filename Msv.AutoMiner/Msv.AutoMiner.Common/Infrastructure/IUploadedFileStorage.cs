using System.IO;
using System.Threading.Tasks;

namespace Msv.AutoMiner.Common.Infrastructure
{
    public interface IUploadedFileStorage
    {
        Task SaveAsync(string name, Stream contentsStream);
        Stream Load(string name);
        string[] Search(string pattern);
    }
}

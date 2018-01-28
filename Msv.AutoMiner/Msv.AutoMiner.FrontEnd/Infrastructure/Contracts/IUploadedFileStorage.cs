using System.IO;
using System.Threading.Tasks;

namespace Msv.AutoMiner.FrontEnd.Infrastructure.Contracts
{
    public interface IUploadedFileStorage
    {
        Task SaveAsync(string name, Stream contentsStream);
        Stream Load(string name);
        bool Exists(string name);
    }
}

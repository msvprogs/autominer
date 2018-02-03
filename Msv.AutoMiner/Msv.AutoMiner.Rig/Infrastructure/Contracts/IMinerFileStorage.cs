using System.IO;

namespace Msv.AutoMiner.Rig.Infrastructure.Contracts
{
    public interface IMinerFileStorage
    {
        string Save(Stream zipStream, string name, int versionId, string mainExecutable);
        string GetPath(int versionId);
        void Delete(int versionId);
    }
}

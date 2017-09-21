using System.Collections.Generic;

namespace Msv.AutoMiner.Rig.System.Contracts
{
    public interface IEnvironmentVariableCreator
    {
        IDictionary<string, string> Create();
    }
}

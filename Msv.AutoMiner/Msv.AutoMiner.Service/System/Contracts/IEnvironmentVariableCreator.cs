using System.Collections.Generic;

namespace Msv.AutoMiner.Service.System.Contracts
{
    public interface IEnvironmentVariableCreator
    {
        IDictionary<string, string> Create();
    }
}

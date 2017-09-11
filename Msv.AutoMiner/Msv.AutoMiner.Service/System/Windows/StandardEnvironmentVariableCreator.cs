using System.Collections.Generic;
using Msv.AutoMiner.Service.System.Contracts;

namespace Msv.AutoMiner.Service.System.Windows
{
    public class StandardEnvironmentVariableCreator : IEnvironmentVariableCreator
    {
        public virtual IDictionary<string, string> Create()
        {
            return new Dictionary<string, string>
            {
                ["GPU_FORCE_64BIT_PTR"] = "0",
                ["GPU_MAX_HEAP_SIZE"] = "100",
                ["GPU_USE_SYNC_OBJECTS"] = "1",
                ["GPU_MAX_ALLOC_PERCENT"] = "100",
                ["GPU_SINGLE_ALLOC_PERCENT"] = "100"
            };
        }
    }
}

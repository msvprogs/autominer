using System;

namespace Msv.AutoMiner.Rig.Infrastructure.Contracts
{
    public interface IPeriodicTaskDelayProvider
    {
        TimeSpan GetDelay<T>();
    }
}

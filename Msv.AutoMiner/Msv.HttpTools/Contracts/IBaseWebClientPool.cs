using System;

namespace Msv.HttpTools.Contracts
{
    public interface IBaseWebClientPool<T> : IDisposable
        where T : class, IBaseWebClient
    {
        PooledItem<T> Acquire();
        void Return(PooledItem<T> item);
    }
}
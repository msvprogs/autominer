using System;
using Msv.HttpTools.Contracts;

namespace Msv.HttpTools
{
    public class PooledItem<T> : IDisposable 
        where T : class, IBaseWebClient
    {     
        public T Value { get; }

        private bool m_Returned;

        private readonly IBaseWebClientPool<T> m_Pool;

        public PooledItem(IBaseWebClientPool<T> pool, T value)
        {
            m_Pool = pool ?? throw new ArgumentNullException(nameof(pool));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public void Dispose()
        {
            m_Pool.Return(this);
            m_Returned = true;
            GC.SuppressFinalize(this);
        }

        ~PooledItem()
        {
            if (!m_Returned)
                m_Pool.Return(this);
        }
    }
}

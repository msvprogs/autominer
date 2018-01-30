using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Msv.HttpTools.Contracts;

namespace Msv.HttpTools
{
    // Linux's libcurl has problem with disposing TCP connections, so we should pool WebClients to avoid this.
    public class BaseWebClientPool<T> : IBaseWebClientPool<T>
        where T : class, IBaseWebClient
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly TimeSpan M_PoolWaitingTimeout = TimeSpan.FromMinutes(5);

        private readonly Func<T> m_Constructor;
        private readonly T[] m_PoolItems;
        private readonly Queue<T> m_PoolQueue;
        private readonly SemaphoreSlim m_Semaphore;

        public BaseWebClientPool(Func<T> constructor, int poolSize)
        {            
            if (poolSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(poolSize));
            m_Constructor = constructor ?? throw new ArgumentNullException(nameof(constructor));
            m_PoolItems = Enumerable.Range(1, poolSize)
                .Select(x => constructor.Invoke())
                .ToArray();
            m_PoolQueue = new Queue<T>(m_PoolItems);
            m_Semaphore = new SemaphoreSlim(poolSize, poolSize);
        }

        public PooledItem<T> Acquire()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                return new PooledItem<T>(this, m_Constructor.Invoke());

            if (!m_Semaphore.Wait(M_PoolWaitingTimeout))
                throw new TimeoutException("Couldn't get webclient from the pool within the timeout");
            lock (m_PoolQueue)
                return new PooledItem<T>(this, m_PoolQueue.Dequeue());
        }

        public void Return(PooledItem<T> item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                item.Value.Dispose();
                return;
            }
            lock (m_PoolQueue)
            {                
                if (m_PoolQueue.Contains(item.Value))
                    return;
                if (!m_PoolItems.Contains(item.Value))
                    throw new InvalidOperationException("This item doesn't belong to this pool");

                m_PoolQueue.Enqueue(item.Value);
                m_Semaphore.Release();
            }
        }

        public void Dispose()
        {
            foreach (var item in m_PoolItems)
                item.Dispose();

            m_Semaphore.Dispose();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Msv.HttpTools.Contracts;

namespace Msv.HttpTools
{
    public class RoundRobinList<T> : IRoundRobin<T>
    {
        public int Count => m_Elements.Length;

        private readonly T[] m_Elements;

        private int m_Current;

        public RoundRobinList(IEnumerable<T> elements)
        {
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));

            m_Elements = elements.DefaultIfEmpty().ToArray();
        }

        public T GetNext()
            => m_Elements[Math.Abs(Interlocked.Increment(ref m_Current) % m_Elements.Length)];
    }
}

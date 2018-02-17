using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Msv.AutoMiner.Common
{
    public class DelegateEqualityComparer<T> : EqualityComparer<T>
    {
        private readonly Func<T, T, bool> m_EqualsFunc;
        private readonly Func<T, int> m_GetHashCodeFunc;

        public DelegateEqualityComparer(
            [NotNull] Func<T, T, bool> equalsFunc,
            [NotNull] Func<T, int> getHashCodeFunc)
        {
            m_EqualsFunc = equalsFunc ?? throw new ArgumentNullException(nameof(equalsFunc));
            m_GetHashCodeFunc = getHashCodeFunc ?? throw new ArgumentNullException(nameof(getHashCodeFunc));
        }

        public override bool Equals(T x, T y)
            => m_EqualsFunc.Invoke(x, y);

        public override int GetHashCode(T obj)
            => m_GetHashCodeFunc.Invoke(obj);
    }
}

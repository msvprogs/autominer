using System;

namespace Msv.HttpTools
{
    public class ProxyInfo
    {
        private const int MaxSequentialFails = 3;
        private static readonly TimeSpan M_InactiveInterval = TimeSpan.FromHours(6);

        public Uri Uri { get; }

        private readonly object m_SyncRoot = new object();

        private int m_Fails;
        private DateTime? m_LastFail;

        public ProxyInfo(Uri uri)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }

        public bool CheckIsAlive()
        {
            lock (m_SyncRoot)
            {
                if (m_Fails < MaxSequentialFails)
                    return true;
                if (m_LastFail + M_InactiveInterval >= DateTime.Now)
                    return false;
                RecordSuccess();
                return true;
            }
        }

        public void RecordFailure()
        {
            lock (m_SyncRoot)
            {
                m_Fails++;
                m_LastFail = DateTime.Now;
            }
        }

        public void RecordSuccess()
        {
            lock (m_SyncRoot)
            {
                m_Fails = 0;
                m_LastFail = null;
            }
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Msv.AutoMiner.Common.Log
{
    [Target("MemoryBuffer")]
    public sealed class MemoryBufferTarget : TargetWithLayout
    {
        private static readonly ConcurrentDictionary<string, List<string>> M_Buffers =
            new ConcurrentDictionary<string, List<string>>();

        [RequiredParameter]
        public string BufferName { get; set; }

        public int Size { get; set; } = 20;

        public static string GetBuffer(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (!M_Buffers.TryGetValue(name, out var buffer))
                return string.Empty;
            lock (buffer)
                return string.Join(Environment.NewLine, buffer);
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var record = Layout.Render(logEvent);
            var buffer = M_Buffers.GetOrAdd(BufferName, new List<string>(Size));
            lock (buffer)
            {
                if (buffer.Count >= Size)
                    buffer.RemoveAt(0);
                buffer.Add(record);
            }
        }
    }
}

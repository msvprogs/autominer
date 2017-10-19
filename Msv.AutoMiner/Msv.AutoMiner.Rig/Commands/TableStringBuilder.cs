using System;
using System.Collections.Generic;
using System.Linq;

namespace Msv.AutoMiner.Rig.Commands
{
    public class TableStringBuilder
    {
        private const string ColumnSeparator = "  |  ";

        private readonly string[] m_Headers;
        private readonly List<object[]> m_Values = new List<object[]>();

        public TableStringBuilder(params string[] headers) 
            => m_Headers = headers ?? throw new ArgumentNullException(nameof(headers));

        public void AppendValues(params object[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            m_Values.Add(values);
        }

        public override string ToString()
        {
            var columnData = m_Headers
                .Select((x, i) => new
                {
                    Header = x,
                    Values = m_Values.Select(y => (i < y.Length ? y[i]?.ToString() : null) ?? string.Empty).ToArray()
                })
                .Select(x => new
                {
                    x.Header,
                    x.Values,
                    MaxWidth = x.Values.Select(y => y.Length).Concat(new[]{x.Header.Length}).Max()
                })
                .ToArray();

            var valueStrings = Enumerable.Range(0, m_Values.Count)
                .Select(x => string.Join(ColumnSeparator, columnData.Select(y => y.Values[x].PadRight(y.MaxWidth))))
                .ToArray();
            var headerString = string.Join(ColumnSeparator, columnData.Select(y => y.Header.PadRight(y.MaxWidth)));

            return string.Join(Environment.NewLine, new[] {headerString, new string('-', headerString.Length)}.Concat(valueStrings));
        }
    }
}

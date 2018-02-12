using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Msv.Licensing.Client.Data;

namespace Msv.Licensing.Client
{
    internal class LinuxHardwareDataProvider : HardwareDataProviderBase
    {
        private const string DmiDecode = "dmidecode";
        private const string Indent = "\t";

        private static readonly char[] M_KeyValueSeparator = ":".ToCharArray();

        public override HardwareData GetHardwareData()
        {
            var output = ParseOutput(ReadProcessOutput(DmiDecode)).ToArray();

            var processor = output.FirstOrDefault(x => x.Key.Contains("Processor Information"));
            var motherboard = output.FirstOrDefault(x => x.Key.Contains("Base Board Information"));
            var memory = output.Where(x => x.Key.Contains("Memory Device")).ToArray();

            return new HardwareData
            {
                ProcessorId = processor.Value?
                    .FirstOrDefault(x => x.Key == "ID").Value,
                ProcessorSignature = processor.Value?
                    .FirstOrDefault(x => x.Key == "Signature").Value,
                MotherboardId = motherboard.Value?
                    .FirstOrDefault(x => x.Key == "Serial Number").Value,
                MotherboardProductName = motherboard.Value?
                    .FirstOrDefault(x => x.Key == "Product Name").Value,
                MemoryIds = memory.Select(x => x.Value.FirstOrDefault(y => y.Key == "Serial Number").Value)
                    .Where(x => x != null)
                    .ToArray()
            };
        }

        [Obfuscation(Exclude = true)]
        private static IEnumerable<KeyValuePair<string, KeyValuePair<string, string>[]>> ParseOutput(dynamic output)
        {
            string header = null;
            var contents = new List<KeyValuePair<string, StringBuilder>>();
            foreach (var line in output)
            {
                if (string.IsNullOrWhiteSpace(line) && header == null)
                    continue;

                if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith(Indent))
                    header += " " + line;
                else if (line.StartsWith(Indent + Indent))
                    contents.Last().Value.AppendLine(line);
                else if (line.StartsWith(Indent))
                {
                    string[] parts = line.Split(M_KeyValueSeparator, 2);
                    contents.Add(new KeyValuePair<string, StringBuilder>(
                        parts[0].Trim(), new StringBuilder(parts.ElementAtOrDefault(1)?.Trim())));
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    yield return new KeyValuePair<string, KeyValuePair<string, string>[]>(header, 
                        contents.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.ToString().Trim())).ToArray());
                    header = null;
                    contents.Clear();
                }
            }
        }

        protected override string GetProcessErrorMessage()
            => "Couldn't get hardware info from dmidecode. "
               + "Check that this package is properly installed and is executable by current user (try to run 'dmidecode' from terminal). "
               + "To enable non-root access to the dmidecode info, execute 'chmod +s /usr/sbin/dmidecode' command with root rights.";
    }
}

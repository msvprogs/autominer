using System.Collections.Generic;
using System.Linq;
using Msv.Licensing.Client.Data;

namespace Msv.Licensing.Client
{
    internal class WindowsHardwareDataProvider : HardwareDataProviderBase
    {
        private const string Wmic = "wmic";

        public override HardwareData GetHardwareData()
            => new HardwareData
            {
                ProcessorId = ParseOutput(ReadProcessOutput(Wmic, "cpu get ProcessorId")).First(),
                ProcessorSignature = ParseOutput(ReadProcessOutput(Wmic, "cpu get Caption")).First(),
                MotherboardId = ParseOutput(ReadProcessOutput(Wmic, "baseboard get SerialNumber")).First(),
                MotherboardProductName = ParseOutput(ReadProcessOutput(Wmic, "baseboard get Product")).First(),
                MemoryIds = ParseOutput(ReadProcessOutput(Wmic, "memorychip get serialnumber")).ToArray()
            };

        private static IEnumerable<string> ParseOutput(IEnumerable<string> output)
            => output.Skip(1)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToArray();

        protected override string GetProcessErrorMessage()
            => "Couldn't execute wmic command. Check that WMI is properly working on your system.";
    }
}

using System.Runtime.InteropServices;

namespace Msv.AutoMiner.Rig.System.Video.NVidia.Nvml
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NvmlUtilization
    {
        public uint Gpu;
        public uint Memory;
    }
}

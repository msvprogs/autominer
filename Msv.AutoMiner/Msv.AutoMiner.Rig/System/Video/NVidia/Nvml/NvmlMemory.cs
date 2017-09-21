using System.Runtime.InteropServices;

namespace Msv.AutoMiner.Rig.System.Video.NVidia.Nvml
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NvmlMemory
    {
        public ulong TotalBytes;
        public ulong FreeBytes;
        public ulong UsedBytes;
    }
}

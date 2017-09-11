using System.Runtime.InteropServices;

namespace Msv.AutoMiner.Service.Video.NVidia.Nvml
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NvmlMemory
    {
        public ulong TotalBytes;
        public ulong FreeBytes;
        public ulong UsedBytes;
    }
}

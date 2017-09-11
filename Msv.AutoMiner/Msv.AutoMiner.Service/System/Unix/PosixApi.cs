using System.Runtime.InteropServices;

namespace Msv.AutoMiner.Service.System.Unix
{
    public static class PosixApi
    {
        private const string LibC = "libc";

        [DllImport(LibC, EntryPoint = "kill", SetLastError = true)]
        public static extern int Kill(int pid, int sig);
    }
}
using System.Runtime.InteropServices;

namespace Msv.AutoMiner.Service.System.Unix
{
    public static class MonoApi
    {
        private const string Lib = "MonoPosixHelper";

        [DllImport(Lib, EntryPoint = "Mono_Posix_FromSignum")]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        public static extern int FromSignum(Signals value, out int rval);
    }
}

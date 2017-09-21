using System;
using System.Diagnostics;
using Msv.AutoMiner.Rig.System.Contracts;

namespace Msv.AutoMiner.Rig.System.Unix
{
    public class UnixProcessStopper : IProcessStopper
    {
        private static readonly int M_SigInt;

        static UnixProcessStopper()
        {
            if (MonoApi.FromSignum(Signals.SigInt, out M_SigInt) != 0)
                throw new InvalidOperationException("Invalid signal number");
        }

        public bool StopProcess(Process process) 
            => PosixApi.Kill(process.Id, M_SigInt) == 0;
    }
}

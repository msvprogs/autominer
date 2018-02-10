using System;

namespace Msv.Licensing.Client
{
    public class LicenseCorruptException : ApplicationException
    {
        public LicenseCorruptException()
            : base("License file is corrupt")
        { }
    }
}

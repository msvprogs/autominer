using System;

namespace Msv.Licensing.Client
{
    public class LicenseExpiredException : ApplicationException
    {
        public LicenseExpiredException()
            : base("License has expired")
        { }
    }
}
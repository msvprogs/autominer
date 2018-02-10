using System;

namespace Msv.Licensing.Client
{
    public class LicenseIsForDifferentApplicationException : ApplicationException
    {
        public LicenseIsForDifferentApplicationException()
            : base("License is for different application")
        { }
    }
}
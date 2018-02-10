using System;
using Msv.Licensing.Common;

namespace Msv.Licensing.Packer
{
    public class PlainPublicKeyProvider : IPublicKeyProvider
    {
        public dynamic Provide()
            => Convert.FromBase64String(
                "BgIAAACkAABSU0ExAAQAAAEAAQDZdm20wFjURUZCUBa6ooSe8zbNqUG79tTyLdXQ5ElI1VGEPDPJrFInoRGzWM0jNdVhmx2Tr/O5wV7eBIx3o5K47EuSpqEGkOogM4kGQZXo41i44tjmeTdoylPFh/w0DuPcz/+1wXVXYHkIscyYJSnt/yvomHtsxG7gqU3+6Dbl0A==");
    }
}

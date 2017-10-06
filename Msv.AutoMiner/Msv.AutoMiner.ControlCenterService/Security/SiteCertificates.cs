﻿using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Msv.AutoMiner.ControlCenterService.Security
{
    public static class SiteCertificates
    {
        public static IReadOnlyDictionary<int, X509Certificate2> PortCertificates { get; }

        static SiteCertificates()
        {
            PortCertificates = new Dictionary<int, X509Certificate2>
            {
                [6283] = new X509Certificate2(File.ReadAllBytes("controlService.pfx"), "vl01fgNUNRFWttb37yst", X509KeyStorageFlags.Exportable),
                [6284] = new X509Certificate2(File.ReadAllBytes("controlCenterEx1.pfx"), "12345", X509KeyStorageFlags.Exportable)
            };
        }
    }
}
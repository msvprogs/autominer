using System;
using System.IO;
using Msv.Licensing.Common;

namespace Msv.Licensing.Issuer
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine("License issuing software for Msv.AutoMiner");
            Console.Write("Enter license ID: ");
            var licenseId = Console.ReadLine();
            Console.Write("Enter hardware ID: ");
            var hardwareId = Console.ReadLine();
            Console.Write("Enter owner name: ");
            var ownerName = Console.ReadLine();

            Console.Write("Choose period: 1 - 7 days, 2 - until April 2018, 3 - unlimited: ");
            int period;
            while (!int.TryParse(Console.ReadLine(), out period)
                    || period < 1 
                    || period > 3)
                Console.WriteLine("Invalid number");

            DateTime? expires = null;
            switch (period)
            {
                case 1:
                    expires = DateTime.UtcNow.Date.AddDays(7);
                    break;
                case 2:
                    expires = new DateTime(2018, 04, 01);
                    break;
            }

            var licenseData = new LicenseData
            {
                ApplicationName = "Msv.AutoMiner",
                Expires = expires,
                HardwareId = hardwareId,
                Issued = DateTime.UtcNow,
                LicenseId = licenseId,
                Owner = ownerName
            };
            var signer = new LicenseDataSigner();
            File.WriteAllBytes($"license_service_{ownerName}_{licenseData.Issued:yyyyMMdd}.msvlic", signer.Sign(licenseData));

            licenseData.ApplicationName = "Msv.AutoMiner.Rig";
            licenseData.SkipHardwareIdValidation = true;
            File.WriteAllBytes($"license_rig_{ownerName}_{licenseData.Issued:yyyyMMdd}.msvlic", signer.Sign(licenseData));

            Console.WriteLine("License files created successfully.");
            Console.ReadKey();
        }
    }
}

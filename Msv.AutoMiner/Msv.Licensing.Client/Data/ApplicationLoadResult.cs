using System;

namespace Msv.Licensing.Client.Data
{
    public class ApplicationLoadResult
    {
        public ApplicationLoadStatus Status { get; }
        public string Message { get; }
        public Exception Error { get; }

        internal ApplicationLoadResult(ApplicationLoadStatus status, string message = null, Exception error = null)
        {
            Status = status;
            switch (status)
            {
                case ApplicationLoadStatus.ApplicationNotFound:
                    Message = "Packed file with licensed application not found. ";
                    break;
                case ApplicationLoadStatus.LicenseCorrupt:
                    Message = "Application license file is corrupt (invalid signature or format). ";
                    break;
                case ApplicationLoadStatus.LicenseExpired:
                    Message = "The license for this application has expired. ";
                    break;
                case ApplicationLoadStatus.LicenseIsForOtherApplication:
                    Message = "The license file you've provided is for other application. ";
                    break;
                case ApplicationLoadStatus.LicenseNotFound:
                    Message = "License file wasn't found. Please acquire the license from the developers. ";
                    break;
                case ApplicationLoadStatus.UnknownError:
                    Message = "Unknown error";
                    break;
            }

            Message += message;
            Error = error;
        }
    }
}

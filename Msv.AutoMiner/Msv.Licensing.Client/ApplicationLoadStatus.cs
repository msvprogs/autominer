namespace Msv.Licensing.Client
{
    public enum ApplicationLoadStatus
    {
        Success,
        LicenseNotFound,
        LicenseCorrupt,
        LicenseExpired,
        LicenseIsForOtherApplication,
        ApplicationNotFound,
        UnknownError
    }
}

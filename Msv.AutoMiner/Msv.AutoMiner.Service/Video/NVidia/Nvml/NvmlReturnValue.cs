namespace Msv.AutoMiner.Service.Video.NVidia.Nvml
{
    public enum NvmlReturnValue
    {
        Success = 0,
        ErrorUninitialized = 1,
        ErrorInvalidArgument = 2,
        ErrorNotSupported = 3,
        ErrorNoPermission = 4,
        ErrorAlreadyInitialized = 5,
        ErrorNotFound = 6,
        ErrorInsufficientSize = 7,
        ErrorInsufficientPower = 8,
        ErrorDriverNotLoaded = 9,
        ErrorTimeout = 10,
        ErrorUnknown = 999
    };
}
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Msv.AutoMiner.Rig.System.Video.NVidia.Nvml
{
    public static class NativeNvml
    {
        public const int DeviceInforomVersionBufferSize = 16;
        public const int DeviceNameBufferSize = 64;
        public const int DeviceSerialBufferSize = 30;
        public const int DeviceUuidBufferSize = 80;
        public const int DeviceVbiosVersionBufferSize = 32;
        public const int SystemDriverVersionBufferSize = 80;
        public const int SystemNvmlVersionBufferSize = 80;
 
        private const string DllName = "nvml";

        [DllImport(DllName, EntryPoint = "nvmlInit")]
        public static extern NvmlReturnValue Initialize();

        [DllImport(DllName, EntryPoint = "nvmlShutdown")]
        public static extern NvmlReturnValue Shutdown();

        [DllImport(DllName, EntryPoint = "nvmlSystemGetDriverVersion")]
        public static extern NvmlReturnValue GetDriverVersion(StringBuilder version, int maxLength);

        [DllImport(DllName, EntryPoint = "nvmlDeviceGetCount")]
        public static extern NvmlReturnValue DeviceGetCount(out int count);

        [DllImport(DllName, EntryPoint = "nvmlDeviceGetHandleByIndex")]
        public static extern NvmlReturnValue DeviceGetHandleByIndex(int index, out IntPtr deviceHandle);

        [DllImport(DllName, EntryPoint = "nvmlDeviceGetClockInfo")]
        public static extern NvmlReturnValue DeviceGetClockInfo(IntPtr deviceHandle, NvmlClockType type, out uint clocksInMhz);

        [DllImport(DllName, EntryPoint = "nvmlDeviceGetMaxClockInfo")]
        public static extern NvmlReturnValue DeviceGetMaxClockInfo(IntPtr deviceHandle, NvmlClockType type, out uint clocksInMhz);

        [DllImport(DllName, EntryPoint = "nvmlDeviceGetFanSpeed")]
        public static extern NvmlReturnValue DeviceGetFanSpeed(IntPtr deviceHandle, out uint speedInPercent);

        [DllImport(DllName, EntryPoint = "nvmlDeviceGetMemoryInfo")]
        public static extern NvmlReturnValue DeviceGetMemoryInfo(IntPtr deviceHandle, ref NvmlMemory memoryInfo);

        [DllImport(DllName, EntryPoint = "nvmlDeviceGetName")]
        public static extern NvmlReturnValue DeviceGetName(IntPtr deviceHandle, StringBuilder name, int maxLength);

        [DllImport(DllName, EntryPoint = "nvmlDeviceGetPerformanceState")]
        public static extern NvmlReturnValue DeviceGetPerformanceState(IntPtr deviceHandle, out uint state);

        [DllImport(DllName, EntryPoint = "nvmlDeviceGetPowerUsage")]
        public static extern NvmlReturnValue DeviceGetPowerUsage(IntPtr deviceHandle, out uint powerInMilliwatts);

        [DllImport(DllName, EntryPoint = "nvmlDeviceGetTemperature")]
        public static extern NvmlReturnValue DeviceGetTemperature(
            IntPtr deviceHandle, NvmlTemperatureSensors sensor, out uint temperature);

        [DllImport(DllName, EntryPoint = "nvmlDeviceGetTemperatureThreshold")]
        public static extern NvmlReturnValue DeviceGetTemperatureThreshold(
            IntPtr deviceHandle, NvmlTemperatureSensors sensor, out uint temperature);

        [DllImport(DllName, EntryPoint = "nvmlDeviceGetUtilizationRates")]
        public static extern NvmlReturnValue DeviceGetUtilizationRates(IntPtr deviceHandle, ref NvmlUtilization utilization);

        [DllImport(DllName, EntryPoint = "nvmlDeviceGetVbiosVersion")]
        public static extern NvmlReturnValue DeviceGetVbiosVersion(IntPtr deviceHandle, StringBuilder version, int maxLength);

        [DllImport(DllName, EntryPoint = "nvmlDeviceGetPowerManagementLimit")]
        public static extern NvmlReturnValue DeviceGetPowerManagementLimit(IntPtr deviceHandle, out uint limitInMilliwatts);
    }
}

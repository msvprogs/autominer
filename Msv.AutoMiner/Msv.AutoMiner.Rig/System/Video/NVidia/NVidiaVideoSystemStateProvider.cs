using System.Text;
using Msv.AutoMiner.Rig.System.Video.NVidia.Nvml;

namespace Msv.AutoMiner.Rig.System.Video.NVidia
{
    public class NVidiaVideoSystemStateProvider : IVideoSystemStateProvider
    {
        public bool CanUse { get; }

        public NVidiaVideoSystemStateProvider()
        {
            try
            {
                if (NativeNvml.Initialize() != NvmlReturnValue.Success)
                    return;
                CanUse = true;
                NativeNvml.Shutdown();
            }
            catch
            {
                CanUse = false;
            }
        }

        public VideoSystemState GetState()
        {
            ThrowOnFatalError(NativeNvml.Initialize());
            try
            {
                return BuildStateInfo();
            }
            finally
            {
                NativeNvml.Shutdown();
            }
        }

        private static VideoSystemState BuildStateInfo()
        {
            var state = new VideoSystemState();
            var versionBuilder = new StringBuilder(NativeNvml.SystemDriverVersionBufferSize);
            ThrowOnFatalError(NativeNvml.GetDriverVersion(versionBuilder, NativeNvml.SystemDriverVersionBufferSize));
            state.DriverVersion = versionBuilder.ToString();

            if (!ThrowOnFatalError(NativeNvml.DeviceGetCount(out var deviceCount)))
            {
                state.AdapterStates = new VideoAdapterState[0];
                return state;
            }
            state.AdapterStates = new VideoAdapterState[deviceCount];
            for (var i = 0; i < deviceCount; i++)
            {
                if (!ThrowOnFatalError(NativeNvml.DeviceGetHandleByIndex(i, out var handle)))
                    return state;
                var nameBuilder = new StringBuilder(NativeNvml.DeviceNameBufferSize);
                var adapterState = state.AdapterStates[i] = new VideoAdapterState {Index = i};
                ThrowOnFatalError(NativeNvml.DeviceGetName(handle, nameBuilder, NativeNvml.DeviceNameBufferSize));
                adapterState.Name = nameBuilder.ToString();
                var vbiosVersionBuilder = new StringBuilder(NativeNvml.DeviceVbiosVersionBufferSize);
                ThrowOnFatalError(NativeNvml.DeviceGetVbiosVersion(handle, vbiosVersionBuilder, NativeNvml.DeviceVbiosVersionBufferSize));
                adapterState.VbiosVersion = vbiosVersionBuilder.ToString();

                if (ThrowOnFatalError(NativeNvml.DeviceGetClockInfo(handle, NvmlClockType.Sm, out var returnedValue)))
                    adapterState.GpuClocksMhz = returnedValue;
                if (ThrowOnFatalError(NativeNvml.DeviceGetClockInfo(handle, NvmlClockType.Memory, out returnedValue)))
                    adapterState.MemoryClocksMhz = returnedValue;
                if (ThrowOnFatalError(NativeNvml.DeviceGetMaxClockInfo(handle, NvmlClockType.Sm, out returnedValue)))
                    adapterState.GpuMaxClocksMhz = returnedValue;
                if (ThrowOnFatalError(
                    NativeNvml.DeviceGetMaxClockInfo(handle, NvmlClockType.Memory, out returnedValue)))
                    adapterState.MemoryMaxClocksMhz = returnedValue;
                if (ThrowOnFatalError(
                    NativeNvml.DeviceGetTemperature(handle, NvmlTemperatureSensors.Gpu, out returnedValue)))
                    adapterState.Temperature = returnedValue;
                if (ThrowOnFatalError(
                    NativeNvml.DeviceGetTemperatureThreshold(handle, NvmlTemperatureSensors.Gpu, out returnedValue)))
                    adapterState.MaxTemperature = returnedValue;
                if (ThrowOnFatalError(NativeNvml.DeviceGetPowerManagementLimit(handle, out returnedValue)))
                    adapterState.PowerLimit = returnedValue / 1000m;
                if (ThrowOnFatalError(NativeNvml.DeviceGetPowerUsage(handle, out returnedValue)))
                    adapterState.PowerUsage = returnedValue / 1000m;
                if (ThrowOnFatalError(NativeNvml.DeviceGetFanSpeed(handle, out returnedValue)))
                    adapterState.FanSpeed = returnedValue;
                if (ThrowOnFatalError(NativeNvml.DeviceGetPerformanceState(handle, out returnedValue)))
                    adapterState.PerformanceState = returnedValue;

                var memory = new NvmlMemory();
                ThrowOnFatalError(NativeNvml.DeviceGetMemoryInfo(handle, ref memory));
                adapterState.TotalMemoryMb = (uint) (memory.TotalBytes / 1024 / 1024);
                adapterState.UsedMemoryMb = (uint) (memory.UsedBytes / 1024 / 1024);

                var utilization = new NvmlUtilization();
                ThrowOnFatalError(NativeNvml.DeviceGetUtilizationRates(handle, ref utilization));
                adapterState.GpuUtilization = utilization.Gpu;
            }
            return state;
        }

        private static bool ThrowOnFatalError(NvmlReturnValue returnValue)
        {
            switch (returnValue)
            {
                case NvmlReturnValue.Success:
                    return true;
                case NvmlReturnValue.ErrorNotSupported:
                    return false;
                default:
                    throw new NvmlException(returnValue);
            }
        }
    }
}

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Msv.AutoMiner.Rig.System.Contracts;
using Msv.AutoMiner.Service.Infrastructure.Contracts;
using NLog;

namespace Msv.AutoMiner.Rig.Infrastructure
{
    public class ProcessWrapper : IProcessWrapper
    {
        private static readonly TimeSpan M_GentleStopTimeout = TimeSpan.FromMinutes(1.5);
        private static readonly Regex M_ConsoleColorInfoRegex = new Regex(@"\e\[.+?m", RegexOptions.Singleline);
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        private readonly Process m_Process;
        private readonly IProcessStopper m_ProcessStopper;
        private readonly IMinerOutputProcessor m_OutputProcessor;
        private readonly bool m_ListenOutput;
        private readonly IChildProcessTracker m_ProcessTracker;
        private volatile bool m_Stopped;

        public bool IsAlive => !m_Process.HasExited;

        public event EventHandler Exited;

        public ProcessWrapper(
            FileInfo file, 
            string arguments, 
            IEnvironmentVariableCreator variableCreator,
            IProcessStopper processStopper, 
            IMinerOutputProcessor outputProcessor,
            IChildProcessTracker processTracker)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));
            if (variableCreator == null)
                throw new ArgumentNullException(nameof(variableCreator));

            m_ProcessStopper = processStopper ?? throw new ArgumentNullException(nameof(processStopper));
            m_OutputProcessor = outputProcessor;
            m_ProcessTracker = processTracker ?? throw new ArgumentNullException(nameof(processTracker));

            m_ListenOutput = outputProcessor != null;
            m_Process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = file.FullName,
                    Arguments = arguments,
                    WorkingDirectory = Path.GetDirectoryName(file.FullName) ?? Directory.GetCurrentDirectory(),
                    UseShellExecute = false,
                    RedirectStandardInput = m_ListenOutput,
                    RedirectStandardOutput = m_ListenOutput,
                    RedirectStandardError = m_ListenOutput
                },
                EnableRaisingEvents = true
            };
            if (m_ListenOutput)
            {
                m_Process.OutputDataReceived += LogOutput;
                m_Process.ErrorDataReceived += LogOutput;
            }
            foreach (var variable in variableCreator.Create())
                m_Process.StartInfo.EnvironmentVariables[variable.Key] = variable.Value;
            m_Process.Exited += (s, e) =>
            {
                if (!m_Stopped)
                    Exited?.Invoke(this, EventArgs.Empty);
            };
        }

        public int Start()
        {
            if (!m_Process.Start())
                throw new InvalidOperationException("Process didn't start");
            m_ProcessTracker.StartTracking(m_Process);
            if (!m_ListenOutput)
                return m_Process.Id;
            m_Process.BeginErrorReadLine();
            m_Process.BeginOutputReadLine();
            return m_Process.Id;
        }

        public void Stop(bool forcefully)
        { 
            if (m_Stopped || m_Process.HasExited)
                return;
            if (m_ListenOutput)
            {
                m_Process.CancelErrorRead();
                m_Process.CancelOutputRead();
            }
            m_Stopped = true;
            var processName = m_Process.ProcessName;
            var pid = m_Process.Id;
            if (!forcefully)
            {
                M_Logger.Debug($"Trying to terminate process {processName} (PID={pid}) gently...");
                if (m_ProcessStopper.StopProcess(m_Process))
                    if (m_Process.WaitForExit((int)M_GentleStopTimeout.TotalMilliseconds))
                    {
                        M_Logger.Info(
                            $"Process {processName} (PID={pid}) has been gently terminated");
                        return;
                    }
                    else
                        M_Logger.Warn(
                            $"Process {processName} (PID={pid}) didn't stop in {(int)M_GentleStopTimeout.TotalSeconds} seconds");
                else
                    M_Logger.Error($"Gentle termination of process {processName} (PID={pid}) failed, killing it", new Win32Exception());
            }
            try
            {
                m_Process.Kill();
                M_Logger.Warn($"Process {processName} (PID={pid}) has been brutally killed.");
            }
            catch (Exception ex)
            {
                M_Logger.Fatal(ex, $"Couldn't kill {processName} (PID={pid}). The process may had become zombie");
            }
        }

        public void Dispose()
        {
            if (!m_Process.HasExited)
                Stop(false);
            if (m_ListenOutput)
            {
                m_Process.OutputDataReceived -= LogOutput;
                m_Process.ErrorDataReceived -= LogOutput;
            }
            m_Process.Dispose();
        }

        private void LogOutput(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                m_OutputProcessor.Write(M_ConsoleColorInfoRegex.Replace(e.Data, string.Empty));
        }
    }
}

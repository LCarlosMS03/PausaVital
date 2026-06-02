using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace PausaVital.Services
{
    public sealed class BackendProcessManager : IDisposable
    {
        private readonly ApiService apiService;
        private Process? backendProcess;

        public BackendProcessManager(ApiService apiService)
        {
            this.apiService = apiService;
        }

        public async Task<bool> EnsureBackendIsRunningAsync()
        {
            if (await apiService.CheckHealthAsync())
            {
                return true;
            }

            string backendDirectory = GetBackendDirectory();

            if (!Directory.Exists(backendDirectory))
            {
                return false;
            }

            // Look exclusively for the compiled Python executable
            string executablePath = Path.Combine(backendDirectory, "PausaVitalBackend.exe");

            if (File.Exists(executablePath))
            {
                backendProcess = TryStartBackend(backendDirectory, executablePath, "");
            }

            if (backendProcess is null)
            {
                return false;
            }

            for (int attempt = 0; attempt < 10; attempt++)
            {
                await Task.Delay(500);

                if (await apiService.CheckHealthAsync())
                {
                    return true;
                }

                if (backendProcess.HasExited)
                {
                    return false;
                }
            }

            return false;
        }

        private static string GetBackendDirectory()
        {
            string appDirectory = AppContext.BaseDirectory;

            string publishedBackendPath = Path.Combine(appDirectory, "Backend");
            if (Directory.Exists(publishedBackendPath))
            {
                return publishedBackendPath;
            }

            string developmentBackendPath = Path.GetFullPath(Path.Combine(appDirectory, "..", "..", "..", "Backend"));
            return developmentBackendPath;
        }

        private static Process? TryStartBackend(string workingDirectory, string executable, string arguments)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                return Process.Start(startInfo);
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            try
            {
                if (backendProcess is not null && !backendProcess.HasExited)
                {
                    backendProcess.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                // Ignore shutdown errors so the WPF app can close cleanly.
            }
            finally
            {
                backendProcess?.Dispose();
                backendProcess = null;
            }
        }
    }
}
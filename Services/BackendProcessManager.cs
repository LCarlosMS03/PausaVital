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

            backendProcess = TryStartBackend(backendDirectory, "py", "-m uvicorn main:app --host 127.0.0.1 --port 8000");
            backendProcess ??= TryStartBackend(backendDirectory, "python", "-m uvicorn main:app --host 127.0.0.1 --port 8000");
            backendProcess ??= TryStartBackend(backendDirectory, "python3", "-m uvicorn main:app --host 127.0.0.1 --port 8000");

            if (backendProcess is null)
            {
                return false;
            }

            // Give Uvicorn a short window to initialize before checking /health again.
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

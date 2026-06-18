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

        private void ExtractBackendResource()
        {
            string targetFolder = Path.Combine(AppContext.BaseDirectory, "Backend");
            string targetPath = Path.Combine(targetFolder, "PausaVitalBackend.exe");

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            if (!File.Exists(targetPath))
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();

                string resourceName = "PausaVital.PausaVitalBackend.exe";

                using (Stream? resourceStream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (resourceStream == null) return;

                    using (FileStream fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
                    {
                        resourceStream.CopyTo(fileStream);
                    }
                }
            }
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

            string executablePath = Path.Combine(backendDirectory, "PausaVitalBackend.exe");

            if (backendProcess is not null && !backendProcess.HasExited)
            {
                return await WaitForBackendHealthAsync();
            }

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

        private async Task<bool> WaitForBackendHealthAsync()
        {
            for (int attempt = 0; attempt < 10; attempt++)
            {
                await Task.Delay(500);

                if (await apiService.CheckHealthAsync())
                {
                    return true;
                }

                if (backendProcess is not null && backendProcess.HasExited)
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
            }
            finally
            {
                backendProcess?.Dispose();
                backendProcess = null;
            }
        }
    }
}
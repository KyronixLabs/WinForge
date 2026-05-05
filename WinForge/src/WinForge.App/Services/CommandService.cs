using System.IO;
using System.Diagnostics;
using System.Text;

namespace WinForge.Services;

public sealed class CommandService
{
    public async Task<string> RunPowerShellAsync(string script, CancellationToken cancellationToken = default)
    {
        AppPaths.Ensure();

        var scriptPath = Path.Combine(AppPaths.Logs, $"winforge-script-{DateTime.Now:yyyyMMdd-HHmmss-fff}.ps1");
        await File.WriteAllTextAsync(scriptPath, script, Encoding.UTF8, cancellationToken);

        var info = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = new Process { StartInfo = info, EnableRaisingEvents = true };
        var output = new StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data)) output.AppendLine(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data)) output.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync(cancellationToken);

        output.AppendLine($"Exit code: {process.ExitCode}");
        return output.ToString();
    }
}

using System.IO;
using Microsoft.Win32;

namespace WinForge.Services;

public sealed class SystemStatusService
{
    public IReadOnlyList<(string Name, string Value, string State)> GetStatusCards()
    {
        AppPaths.Ensure();
        return new List<(string Name, string Value, string State)>
        {
            ("Computer", Environment.MachineName, "Ready"),
            ("Administrator", AdminService.IsAdministrator() ? "Active" : "Limited", AdminService.IsAdministrator() ? "Ready" : "Check"),
            ("Windows", GetWindowsName(), "Ready"),
            ("Processor", GetProcessorName(), "Info"),
            ("Memory", GetMemoryText(), "Info"),
            ("System drive", GetSystemDriveText(), "Info"),
            ("Local network", GetLocalIpText(), "Info"),
            ("Uptime", GetUptimeText(), "Info")
        };
    }

    public string GetDetailedReport()
    {
        var lines = new List<string>
        {
            $"Captured: {DateTime.Now:g}",
            $"Computer: {Environment.MachineName}",
            $"User: {Environment.UserName}",
            $"Administrator: {AdminService.IsAdministrator()}",
            $"Windows: {GetWindowsName()}",
            $"Processor: {GetProcessorName()}",
            $"Memory: {GetMemoryText()}",
            $"System drive: {GetSystemDriveText()}",
            $"WinForge data: {AppPaths.Root}"
        };

        return string.Join(Environment.NewLine, lines);
    }

    private static string GetWindowsName()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var name = Convert.ToString(key?.GetValue("ProductName"));
            var build = Convert.ToString(key?.GetValue("CurrentBuildNumber"));
            var display = Convert.ToString(key?.GetValue("DisplayVersion"));
            if (!string.IsNullOrWhiteSpace(name))
            {
                return string.IsNullOrWhiteSpace(display) ? $"{name} build {build}" : $"{name} {display} build {build}";
            }
        }
        catch
        {
        }

        return Environment.OSVersion.VersionString;
    }

    private static string GetProcessorName()
    {
        var value = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
        return string.IsNullOrWhiteSpace(value) ? "Unknown" : value;
    }

    private static string GetMemoryText()
    {
        var bytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        return bytes > 0 ? $"{bytes / 1024d / 1024d / 1024d:0.0} GB available to .NET" : "Unknown";
    }

    private static string GetSystemDriveText()
    {
        var root = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
        var drive = new DriveInfo(root);
        return $"{drive.AvailableFreeSpace / 1024d / 1024d / 1024d:0.0} GB free";
    }

    private static string GetLocalIpText()
    {
        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            var address = host.AddressList.FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !System.Net.IPAddress.IsLoopback(x));
            return address?.ToString() ?? "Unavailable";
        }
        catch
        {
            return "Unavailable";
        }
    }

    private static string GetUptimeText()
    {
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        return uptime.TotalDays >= 1
            ? $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m"
            : $"{uptime.Hours}h {uptime.Minutes}m";
    }
}

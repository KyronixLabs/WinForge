using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using WinForge.Services;

namespace WinForge.Views;

public partial class LogsView : UserControl
{
    public LogsView()
    {
        InitializeComponent();
        RefreshLogs();
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e) => RefreshLogs();

    private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
    {
        AppPaths.Ensure();
        Process.Start(new ProcessStartInfo { FileName = AppPaths.Logs, UseShellExecute = true });
    }

    private void RefreshLogs()
    {
        AppPaths.Ensure();
        var files = Directory.GetFiles(AppPaths.Logs, "*.log").OrderByDescending(File.GetLastWriteTime).Take(8).ToList();
        if (files.Count == 0)
        {
            LogsBox.Text = "No logs have been created yet.";
            return;
        }

        var parts = new List<string>();
        foreach (var file in files)
        {
            parts.Add($"File: {file}");
            parts.Add(File.ReadAllText(file));
            parts.Add(new string('=', 80));
        }

        LogsBox.Text = string.Join(Environment.NewLine, parts);
    }
}

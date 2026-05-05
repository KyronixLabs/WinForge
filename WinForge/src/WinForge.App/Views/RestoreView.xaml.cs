using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using WinForge.Models;
using WinForge.Services;

namespace WinForge.Views;

public partial class RestoreView : UserControl
{
    private readonly OptimisationService _service;

    public RestoreView(OptimisationService service)
    {
        _service = service;
        InitializeComponent();
        LoadSnapshots();
    }

    private void LoadSnapshots()
    {
        SnapshotList.Items.Clear();
        foreach (var snapshot in _service.GetSnapshots())
        {
            var item = new ListBoxItem
            {
                Content = $"{snapshot.CreatedAt:g}    {snapshot.AppliedCount} changes    {snapshot.Name}",
                Tag = snapshot
            };
            SnapshotList.Items.Add(item);
        }

        OutputBox.Text = SnapshotList.Items.Count == 0
            ? "No optimisation snapshots were found."
            : "Select a snapshot to revert it.";
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e) => LoadSnapshots();

    private async void RevertSelectedButton_Click(object sender, RoutedEventArgs e)
    {
        if (SnapshotList.SelectedItem is not ListBoxItem { Tag: SnapshotInfo snapshot })
        {
            OutputBox.Text = "Select a snapshot first.";
            return;
        }

        OutputBox.Text = await _service.RevertSnapshotAsync(snapshot.Path);
    }

    private async void RevertLatestButton_Click(object sender, RoutedEventArgs e)
    {
        OutputBox.Text = await _service.RevertLastAsync();
    }

    private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
    {
        AppPaths.Ensure();
        Process.Start(new ProcessStartInfo { FileName = AppPaths.Snapshots, UseShellExecute = true });
    }
}

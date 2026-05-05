using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using WinForge.Models;
using WinForge.Services;

namespace WinForge.Views;

public partial class GamingView : UserControl
{
    private readonly GamingService _gaming = new();

    public GamingView()
    {
        InitializeComponent();
        OutputBox.Text = "Choose the options you want, then apply Maximum FPS Mode or start a temporary gaming session.";
    }

    private async void ApplyGamingButton_Click(object sender, RoutedEventArgs e)
    {
        await RunAsync("Applying Gaming Optimisation...", () => _gaming.ApplyMaximumFpsAsync(ReadOptions()));
    }

    private async void RevertGamingButton_Click(object sender, RoutedEventArgs e)
    {
        await RunAsync("Reverting Gaming Optimisation...", () => _gaming.RevertMaximumFpsAsync());
    }

    private async void ReportButton_Click(object sender, RoutedEventArgs e)
    {
        await RunAsync("Generating Gaming Report...", () => _gaming.GenerateReportAsync());
    }

    private async void StartSessionButton_Click(object sender, RoutedEventArgs e)
    {
        await RunAsync("Starting Gaming Session...", () => _gaming.StartSessionAsync(ReadOptions()));
    }

    private async void EndSessionButton_Click(object sender, RoutedEventArgs e)
    {
        await RunAsync("Ending Gaming Session...", () => _gaming.EndSessionAsync());
    }

    private void ChooseGameButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose game executable",
            Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog() == true)
        {
            GamePathBox.Text = dialog.FileName;
        }
    }

    private async void SetGpuButton_Click(object sender, RoutedEventArgs e)
    {
        await RunAsync("Setting high performance GPU preference...", () => _gaming.SetSelectedGameHighPerformanceAsync(GamePathBox.Text));
    }

    private GamingOptions ReadOptions()
    {
        return new GamingOptions
        {
            EnableGameMode = GameModeBox.IsChecked == true,
            BestPerformancePower = PowerBox.IsChecked == true,
            DisableBackgroundRecording = CaptureBox.IsChecked == true,
            DisableNetworkPowerSaving = NetworkPowerBox.IsChecked == true,
            DisableUsbAndPciePowerSaving = UsbPcieBox.IsChecked == true,
            DisableMouseAcceleration = MouseBox.IsChecked == true,
            FlushDns = DnsFlushBox.IsChecked == true,
            ClearDirectXShaderCache = ShaderCacheBox.IsChecked == true,
            DisableXboxGameBar = XboxGameBarBox.IsChecked == true,
            EnableHardwareGpuScheduling = HagsBox.IsChecked == true,
            DisableVisualEffects = VisualEffectsBox.IsChecked == true,
            ApplyGamingDns = GamingDnsBox.IsChecked == true,
            CloseBackgroundApps = CloseAppsBox.IsChecked == true,
            BackgroundApps = BackgroundAppsBox.Text,
            SelectedGamePath = GamePathBox.Text
        };
    }

    private async Task RunAsync(string status, Func<Task<string>> action)
    {
        IsEnabled = false;
        OutputBox.Text = status;
        try
        {
            OutputBox.Text = await action();
        }
        catch (Exception ex)
        {
            OutputBox.Text = ex.Message;
        }
        finally
        {
            IsEnabled = true;
        }
    }
}

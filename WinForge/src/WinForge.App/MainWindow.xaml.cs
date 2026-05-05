using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WinForge.Services;
using WinForge.Views;

namespace WinForge;

public partial class MainWindow : Window
{
    private readonly NavigationPageFactory _pages;
    private IReadOnlyList<Button> _advancedButtons = Array.Empty<Button>();

    public MainWindow()
    {
        InitializeComponent();

        var optimisation = new OptimisationService();
        var status = new SystemStatusService();

        _pages = new NavigationPageFactory(optimisation, status);
        _advancedButtons = new[] { RestoreButton, ToolsButton };

        AdminStatusText.Text = AdminService.IsAdministrator() ? "Administrator access active" : "Administrator access recommended";
        ApplyAdvancedMode();
        Open(DashboardButton, _pages.Dashboard());
    }

    private void DashboardButton_Click(object sender, RoutedEventArgs e) => Open(DashboardButton, _pages.Dashboard());
    private void OptimiseButton_Click(object sender, RoutedEventArgs e) => Open(OptimiseButton, _pages.Optimise());
    private void GamingButton_Click(object sender, RoutedEventArgs e) => Open(GamingButton, _pages.Gaming());
    private void AppsButton_Click(object sender, RoutedEventArgs e) => Open(AppsButton, _pages.Apps());
    private void StorageButton_Click(object sender, RoutedEventArgs e) => Open(StorageButton, _pages.Storage());
    private void NetworkButton_Click(object sender, RoutedEventArgs e) => Open(NetworkButton, _pages.Network());
    private void SecurityButton_Click(object sender, RoutedEventArgs e) => Open(SecurityButton, _pages.Security());
    private void SystemButton_Click(object sender, RoutedEventArgs e) => Open(SystemButton, _pages.System());
    private void RepairButton_Click(object sender, RoutedEventArgs e) => Open(RepairButton, _pages.Repair());
    private void ProfilesButton_Click(object sender, RoutedEventArgs e) => Open(ProfilesButton, _pages.Profiles());
    private void RestoreButton_Click(object sender, RoutedEventArgs e) => Open(RestoreButton, _pages.Restore());
    private void ToolsButton_Click(object sender, RoutedEventArgs e) => Open(ToolsButton, _pages.Tools());
    private void LogsButton_Click(object sender, RoutedEventArgs e) => Open(LogsButton, _pages.Logs());

    private void RunAsAdminButton_Click(object sender, RoutedEventArgs e)
    {
        if (!AdminService.IsAdministrator())
        {
            AdminService.RestartAsAdministrator();
        }
    }

    private void AdvancedToolsCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        ApplyAdvancedMode();
    }

    private void ApplyAdvancedMode()
    {
        var showAdvanced = AdvancedToolsCheckBox.IsChecked == true;
        foreach (var button in _advancedButtons)
        {
            button.Visibility = showAdvanced ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void Open(Button selectedButton, UserControl page)
    {
        foreach (var button in FindVisualChildren<Button>(this).Where(button => button.Style == TryFindResource("NavButton")))
        {
            button.Background = Brushes.Transparent;
            button.Foreground = (Brush)FindResource("MutedBrush");
        }

        selectedButton.Background = new SolidColorBrush(Color.FromRgb(28, 36, 52));
        selectedButton.Foreground = (Brush)FindResource("AccentBrush");
        PageHost.Content = page;
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
            {
                yield return match;
            }

            foreach (var descendant in FindVisualChildren<T>(child))
            {
                yield return descendant;
            }
        }
    }
}

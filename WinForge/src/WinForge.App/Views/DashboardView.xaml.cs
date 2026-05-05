using System.Windows.Controls;
using System.Windows.Media;
using WinForge.Services;
using System.Windows.Threading;

namespace WinForge.Views;

public partial class DashboardView : UserControl
{
    private readonly SystemStatusService _status;
    private readonly DispatcherTimer _timer = new();

    public DashboardView(SystemStatusService status)
    {
        _status = status;
        InitializeComponent();
        LoadStatus();

        _timer.Interval = TimeSpan.FromSeconds(8);
        _timer.Tick += (_, _) => LoadStatus();
        _timer.Start();
        Unloaded += (_, _) => _timer.Stop();
    }

    private void LoadStatus()
    {
        StatusPanel.Children.Clear();

        foreach (var item in _status.GetStatusCards())
        {
            StatusPanel.Children.Add(CreateCard(item.Name, item.Value, item.State));
        }

        ReportBox.Text = _status.GetDetailedReport();
    }

    private Border CreateCard(string title, string value, string state)
    {
        var accent = (Brush)FindResource("AccentBrush");
        var ink = (Brush)FindResource("InkBrush");
        var muted = (Brush)FindResource("MutedBrush");
        var line = (Brush)FindResource("LineBrush");
        var panel = (Brush)FindResource("PanelBrush");

        var badge = new Border
        {
            CornerRadius = new System.Windows.CornerRadius(10),
            Padding = new System.Windows.Thickness(10, 4, 10, 4),
            Background = state == "Ready" ? new SolidColorBrush(Color.FromRgb(29, 70, 51)) : new SolidColorBrush(Color.FromRgb(74, 53, 25)),
            Child = new TextBlock
            {
                Text = state,
                Foreground = state == "Ready" ? new SolidColorBrush(Color.FromRgb(156, 255, 202)) : accent,
                FontWeight = System.Windows.FontWeights.SemiBold,
                FontSize = 12
            }
        };

        var stack = new StackPanel();
        stack.Children.Add(new TextBlock { Text = title, Foreground = accent, FontSize = 16, FontWeight = System.Windows.FontWeights.SemiBold });
        stack.Children.Add(new TextBlock { Text = value, Foreground = ink, FontSize = 22, FontWeight = System.Windows.FontWeights.SemiBold, Margin = new System.Windows.Thickness(0, 8, 0, 10), TextWrapping = System.Windows.TextWrapping.Wrap });
        stack.Children.Add(badge);

        return new Border
        {
            Width = 245,
            MinHeight = 150,
            Background = panel,
            BorderBrush = line,
            BorderThickness = new System.Windows.Thickness(1),
            CornerRadius = new System.Windows.CornerRadius(18),
            Padding = new System.Windows.Thickness(18),
            Margin = new System.Windows.Thickness(0, 0, 16, 16),
            Child = stack
        };
    }
}

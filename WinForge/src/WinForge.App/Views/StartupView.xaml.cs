using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WinForge.Models;
using WinForge.Services;

namespace WinForge.Views;

public partial class StartupView : UserControl
{
    private readonly StartupService _startupService = new();
    private readonly List<StartupItem> _items = new();
    private readonly List<(StartupItem Item, CheckBox Toggle)> _rows = new();

    public StartupView()
    {
        InitializeComponent();
        ReportBox.Text = "Startup entries appear below. Turn items on or off, then apply the selected changes.";
        LoadItems();
    }

    private void LoadItems()
    {
        _items.Clear();
        _items.AddRange(_startupService.LoadItems());
        BuildItems();
    }

    private void BuildItems()
    {
        ItemsPanel.Children.Clear();
        _rows.Clear();

        var search = SearchBox.Text?.Trim() ?? string.Empty;
        var visibleItems = _items
            .Where(item => string.IsNullOrWhiteSpace(search)
                || item.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || item.Source.Contains(search, StringComparison.OrdinalIgnoreCase)
                || item.Location.Contains(search, StringComparison.OrdinalIgnoreCase)
                || item.Command.Contains(search, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.IsEnabled ? 0 : 1)
            .ThenBy(x => x.Source)
            .ThenBy(x => x.Name)
            .ToList();

        foreach (var item in visibleItems)
        {
            var toggle = new CheckBox
            {
                IsChecked = item.IsEnabled,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(12, 2, 0, 0),
                Foreground = (Brush)FindResource("InkBrush")
            };
            toggle.Checked += (_, _) => item.IsEnabled = true;
            toggle.Unchecked += (_, _) => item.IsEnabled = false;

            var status = new Border
            {
                Background = item.IsEnabled ? new SolidColorBrush(Color.FromRgb(29, 70, 51)) : new SolidColorBrush(Color.FromRgb(74, 53, 25)),
                CornerRadius = new CornerRadius(9),
                Padding = new Thickness(8, 2, 8, 2),
                Margin = new Thickness(0, 0, 8, 0),
                Child = new TextBlock
                {
                    Text = item.IsEnabled ? "Enabled" : "Disabled",
                    Foreground = item.IsEnabled ? new SolidColorBrush(Color.FromRgb(156, 255, 202)) : (Brush)FindResource("AccentBrush"),
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 11
                }
            };

            var header = new StackPanel { Orientation = Orientation.Horizontal };
            header.Children.Add(status);
            header.Children.Add(new TextBlock
            {
                Text = item.Name,
                Foreground = (Brush)FindResource("AccentBrush"),
                FontWeight = FontWeights.SemiBold,
                FontSize = 15,
                VerticalAlignment = VerticalAlignment.Center
            });

            var details = new StackPanel();
            details.Children.Add(header);
            details.Children.Add(new TextBlock
            {
                Text = item.Source,
                Foreground = (Brush)FindResource("MutedBrush"),
                Margin = new Thickness(0, 4, 0, 0),
                FontSize = 12
            });
            details.Children.Add(new TextBlock
            {
                Text = item.Location,
                Foreground = (Brush)FindResource("MutedBrush"),
                Margin = new Thickness(0, 1, 0, 0),
                TextWrapping = TextWrapping.NoWrap,
                TextTrimming = TextTrimming.CharacterEllipsis,
                FontSize = 12
            });
            if (!string.IsNullOrWhiteSpace(item.Command))
            {
                details.Children.Add(new TextBlock
                {
                    Text = item.Command,
                    Foreground = (Brush)FindResource("MutedBrush"),
                    Margin = new Thickness(0, 1, 0, 0),
                    TextWrapping = TextWrapping.NoWrap,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    FontSize = 11
                });
            }

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.Children.Add(details);
            Grid.SetColumn(toggle, 1);
            grid.Children.Add(toggle);

            var border = new Border
            {
                Background = (Brush)FindResource("PanelBrush"),
                BorderBrush = (Brush)FindResource("LineBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 8),
                Child = grid
            };

            ItemsPanel.Children.Add(border);
            _rows.Add((item, toggle));
        }

        if (_rows.Count == 0)
        {
            ItemsPanel.Children.Add(new TextBlock
            {
                Text = "No matching startup entries were found.",
                Foreground = (Brush)FindResource("MutedBrush"),
                Margin = new Thickness(6)
            });
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => BuildItems();

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadItems();
        ReportBox.Text = "Startup list refreshed.";
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var row in _rows)
        {
            row.Item.IsEnabled = row.Toggle.IsChecked == true;
        }

        var result = _startupService.ApplyChanges(_items);
        ReportBox.Text = result;
        LoadItems();
    }

    private void EnableVisibleButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var row in _rows)
        {
            row.Item.IsEnabled = true;
            row.Toggle.IsChecked = true;
        }
        BuildItems();
    }

    private void DisableVisibleButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var row in _rows)
        {
            row.Item.IsEnabled = false;
            row.Toggle.IsChecked = false;
        }
        BuildItems();
    }

    private void OpenStartupFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
    }

    private void ReportButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var row in _rows)
        {
            row.Item.IsEnabled = row.Toggle.IsChecked == true;
        }

        ReportBox.Text = _startupService.BuildReport(_items);
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WinForge.Models;
using WinForge.Services;

namespace WinForge.Views;

public partial class OptimiseView : UserControl
{
    private readonly OptimisationService _service;

    public OptimiseView(OptimisationService service)
    {
        _service = service;
        InitializeComponent();
        BuildOptions();
        UpdateSelectionText();
    }

    private void BuildOptions()
    {
        OptionsPanel.Children.Clear();

        foreach (var group in _service.Options.GroupBy(option => option.Category))
        {
            var section = new StackPanel { Margin = new Thickness(0, 0, 0, 18) };
            section.Children.Add(new TextBlock
            {
                Text = group.Key,
                Foreground = (Brush)FindResource("AccentBrush"),
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 10)
            });

            var wrap = new WrapPanel();
            foreach (var option in group)
            {
                wrap.Children.Add(CreateOptionCard(option));
            }

            section.Children.Add(wrap);
            OptionsPanel.Children.Add(section);
        }
    }

    private Border CreateOptionCard(OptimisationOption option)
    {
        var check = new CheckBox
        {
            Content = option.Title,
            IsChecked = option.IsSelected,
            Foreground = (Brush)FindResource("AccentBrush"),
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 8)
        };

        check.Checked += (_, _) => { option.IsSelected = true; UpdateSelectionText(); };
        check.Unchecked += (_, _) => { option.IsSelected = false; UpdateSelectionText(); };

        var stack = new StackPanel();
        stack.Children.Add(check);
        stack.Children.Add(new TextBlock { Text = option.Summary, Foreground = (Brush)FindResource("InkBrush"), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 6) });
        stack.Children.Add(new TextBlock { Text = option.Details, Foreground = (Brush)FindResource("MutedBrush"), TextWrapping = TextWrapping.Wrap, FontSize = 12 });
        stack.Children.Add(new TextBlock { Text = option.HasRevert ? "Reversible" : "No automatic revert", Foreground = option.HasRevert ? new SolidColorBrush(Color.FromRgb(156, 255, 202)) : (Brush)FindResource("MutedBrush"), Margin = new Thickness(0, 10, 0, 0), FontSize = 12, FontWeight = FontWeights.SemiBold });

        return new Border
        {
            Width = 310,
            MinHeight = 165,
            Background = (Brush)FindResource("PanelBrush"),
            BorderBrush = (Brush)FindResource("LineBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(18),
            Padding = new Thickness(16),
            Margin = new Thickness(0, 0, 16, 16),
            Child = stack
        };
    }

    private async void OptimiseButton_Click(object sender, RoutedEventArgs e)
    {
        var selected = _service.Options.Where(option => option.IsSelected).ToList();
        if (selected.Count == 0)
        {
            OutputBox.Text = "No optimisation options were selected.";
            return;
        }

        var review = "WinForge is ready to apply these changes:" + Environment.NewLine + Environment.NewLine
            + string.Join(Environment.NewLine, selected.Select(option => "• " + option.Title))
            + Environment.NewLine + Environment.NewLine
            + "A restore point request and a WinForge snapshot will be created before changes are applied.";

        var decision = MessageBox.Show(review, "Review selected changes", MessageBoxButton.OKCancel, MessageBoxImage.Information);
        if (decision != MessageBoxResult.OK)
        {
            OutputBox.Text = "Optimisation cancelled before any changes were applied.";
            return;
        }

        SetBusy(true, "Optimising selected items...");
        var result = await _service.ApplySelectedAsync();
        OutputBox.Text = result;
        SetBusy(false, "Optimisation complete.");
    }

    private async void RevertButton_Click(object sender, RoutedEventArgs e)
    {
        SetBusy(true, "Reverting latest run...");
        var result = await _service.RevertLastAsync();
        OutputBox.Text = result;
        SetBusy(false, "Revert complete.");
    }

    private void SafeProfileButton_Click(object sender, RoutedEventArgs e)
    {
        _service.SelectProfile("Safe");
        BuildOptions();
        UpdateSelectionText();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var option in _service.Options)
        {
            option.IsSelected = false;
        }

        BuildOptions();
        UpdateSelectionText();
    }

    private void SetBusy(bool busy, string message)
    {
        OutputBox.Text = message;
        IsEnabled = !busy;
    }

    private void UpdateSelectionText()
    {
        var count = _service.Options.Count(option => option.IsSelected);
        SelectionText.Text = $"{count} selected";
    }
}

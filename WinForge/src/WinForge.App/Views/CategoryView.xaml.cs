using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WinForge.Models;
using WinForge.Services;

namespace WinForge.Views;

public partial class CategoryView : UserControl
{
    private readonly OptimisationService _service;
    private readonly string _category;

    public CategoryView(OptimisationService service, string category, string subtitle)
    {
        _service = service;
        _category = category;
        InitializeComponent();
        TitleText.Text = category;
        SubtitleText.Text = subtitle;
        BuildOptions();
    }

    private IEnumerable<OptimisationOption> PageOptions => _service.Options.Where(option => option.Category == _category);

    private void BuildOptions()
    {
        OptionsPanel.Children.Clear();
        foreach (var option in PageOptions)
        {
            OptionsPanel.Children.Add(CreateOptionCard(option));
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

        check.Checked += (_, _) => option.IsSelected = true;
        check.Unchecked += (_, _) => option.IsSelected = false;

        var stack = new StackPanel();
        stack.Children.Add(check);
        stack.Children.Add(new TextBlock { Text = option.Summary, Foreground = (Brush)FindResource("InkBrush"), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 6) });
        stack.Children.Add(new TextBlock { Text = option.Details, Foreground = (Brush)FindResource("MutedBrush"), TextWrapping = TextWrapping.Wrap, FontSize = 12 });

        return new Border
        {
            Width = 320,
            MinHeight = 150,
            Background = (Brush)FindResource("PanelBrush"),
            BorderBrush = (Brush)FindResource("LineBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(18),
            Padding = new Thickness(16),
            Margin = new Thickness(0, 0, 16, 16),
            Child = stack
        };
    }

    private void SelectAllButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var option in PageOptions) option.IsSelected = true;
        BuildOptions();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var option in PageOptions) option.IsSelected = false;
        BuildOptions();
    }
}

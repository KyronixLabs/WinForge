using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WinForge.Models;
using WinForge.Services;

namespace WinForge.Views;

public partial class ToolPageView : UserControl
{
    private readonly FeatureToolService _service = new();

    public ToolPageView(string title, string subtitle)
    {
        InitializeComponent();
        TitleText.Text = title;
        SubtitleText.Text = subtitle;
        OutputBox.Text = "Choose an action to see results here.";
        BuildActions(_service.GetActions(title));
    }

    private void BuildActions(IReadOnlyList<ToolAction> actions)
    {
        ActionsPanel.Children.Clear();
        foreach (var action in actions)
        {
            ActionsPanel.Children.Add(CreateActionCard(action));
        }
    }

    private Border CreateActionCard(ToolAction action)
    {
        var stack = new StackPanel();
        stack.Children.Add(CreateBadgeRow(action));
        stack.Children.Add(new TextBlock
        {
            Text = action.Title,
            Foreground = (Brush)FindResource("AccentBrush"),
            FontSize = 17,
            FontWeight = FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 10, 0, 0)
        });
        stack.Children.Add(new TextBlock
        {
            Text = action.Description,
            Foreground = (Brush)FindResource("MutedBrush"),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 0)
        });
        stack.Children.Add(CreateRunButton(action));

        return new Border
        {
            Width = 320,
            MinHeight = 178,
            Background = (Brush)FindResource("PanelBrush"),
            BorderBrush = (Brush)FindResource("LineBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(18),
            Padding = new Thickness(16),
            Margin = new Thickness(0, 0, 16, 16),
            Child = stack
        };
    }

    private WrapPanel CreateBadgeRow(ToolAction action)
    {
        var panel = new WrapPanel();
        panel.Children.Add(CreateBadge(action.IsDestructive ? "Careful" : "Safe", action.IsDestructive));
        if (action.RequiresAdmin)
        {
            panel.Children.Add(CreateBadge("Admin", true));
        }
        else
        {
            panel.Children.Add(CreateBadge("Standard", false));
        }
        return panel;
    }

    private Border CreateBadge(string text, bool warning)
    {
        return new Border
        {
            Background = warning ? new SolidColorBrush(Color.FromRgb(74, 53, 25)) : new SolidColorBrush(Color.FromRgb(29, 70, 51)),
            CornerRadius = new CornerRadius(9),
            Padding = new Thickness(9, 3, 9, 3),
            Margin = new Thickness(0, 0, 6, 0),
            Child = new TextBlock
            {
                Text = text,
                Foreground = warning ? (Brush)FindResource("AccentBrush") : new SolidColorBrush(Color.FromRgb(156, 255, 202)),
                FontSize = 12,
                FontWeight = FontWeights.SemiBold
            }
        };
    }

    private Button CreateRunButton(ToolAction action)
    {
        var button = new Button
        {
            Content = "Run action",
            Style = (Style)FindResource(action.IsDestructive ? "DangerButton" : "SecondaryButton"),
            Tag = action,
            Margin = new Thickness(0, 14, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        button.Click += RunButton_Click;
        return button;
    }

    private async void RunButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ToolAction action })
        {
            return;
        }

        IsEnabled = false;
        OutputExpander.IsExpanded = true;
        OutputBox.Text = $"Running {action.Title}...";
        try
        {
            OutputBox.Text = await _service.RunAsync(action);
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

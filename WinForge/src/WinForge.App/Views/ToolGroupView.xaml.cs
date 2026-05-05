using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WinForge.Views;

public sealed record ToolGroupTab(string Title, string Description, Func<UserControl> CreatePage);

public partial class ToolGroupView : UserControl
{
    private readonly IReadOnlyList<ToolGroupTab> _tabs;
    private readonly List<Button> _buttons = new();

    public ToolGroupView(string title, string subtitle, IEnumerable<ToolGroupTab> tabs)
    {
        InitializeComponent();
        TitleText.Text = title;
        SubtitleText.Text = subtitle;
        _tabs = tabs.ToList();
        BuildTabs();

        if (_tabs.Count > 0)
        {
            ActivateTab(0);
        }
    }

    private void BuildTabs()
    {
        TabsPanel.Children.Clear();
        _buttons.Clear();

        for (var index = 0; index < _tabs.Count; index++)
        {
            var tab = _tabs[index];
            var button = new Button
            {
                Content = tab.Title,
                Tag = index,
                Style = (Style)FindResource("SecondaryButton"),
                Margin = new Thickness(0, 0, 10, 0),
                MinWidth = 120
            };
            button.Click += TabButton_Click;
            TabsPanel.Children.Add(button);
            _buttons.Add(button);
        }
    }

    private void TabButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: int index })
        {
            ActivateTab(index);
        }
    }

    private void ActivateTab(int index)
    {
        for (var i = 0; i < _buttons.Count; i++)
        {
            _buttons[i].Background = (Brush)FindResource(i == index ? "SuccessBrush" : "PanelSoftBrush");
            _buttons[i].Foreground = i == index
                ? new SolidColorBrush(Color.FromRgb(8, 18, 12))
                : (Brush)FindResource("InkBrush");
            _buttons[i].BorderBrush = i == index
                ? (Brush)FindResource("SuccessBrush")
                : (Brush)FindResource("LineBrush");
        }

        GroupPageHost.Content = _tabs[index].CreatePage();
    }
}

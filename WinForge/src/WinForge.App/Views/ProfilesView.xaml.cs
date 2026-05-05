using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WinForge.Services;

namespace WinForge.Views;

public partial class ProfilesView : UserControl
{
    private readonly OptimisationService _service;

    public ProfilesView(OptimisationService service)
    {
        _service = service;
        InitializeComponent();
        BuildProfiles();
        RefreshSavedProfiles();
    }

    private void BuildProfiles()
    {
        AddProfile("Safe", "Clean temporary files, flush DNS and keep security enabled.", "Safe");
        AddProfile("Gaming", "Prioritise gaming settings and reduce capture overhead.", "Gaming");
        AddProfile("Low end PC", "Reduce visual overhead and apply light cleanup.", "LowEnd");
        AddProfile("Home server", "Keep the machine awake and network friendly.", "Server");
        AddProfile("Privacy", "Select privacy related settings and basic security checks.", "Privacy");
    }

    private void AddProfile(string title, string summary, string key)
    {
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock { Text = title, Foreground = (Brush)FindResource("AccentBrush"), FontSize = 20, FontWeight = FontWeights.SemiBold });
        stack.Children.Add(new TextBlock { Text = summary, Foreground = (Brush)FindResource("MutedBrush"), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 10, 0, 18) });

        var button = new Button { Content = "Select profile", Style = (Style)FindResource("PrimaryButton") };
        button.Click += (_, _) =>
        {
            _service.SelectProfile(key);
            MessageBox.Show("Profile selected. Open Optimise to review and apply the selected options.", "WinForge", MessageBoxButton.OK, MessageBoxImage.Information);
        };
        stack.Children.Add(button);

        ProfilesPanel.Children.Add(new Border
        {
            Width = 310,
            MinHeight = 190,
            Background = (Brush)FindResource("PanelBrush"),
            BorderBrush = (Brush)FindResource("LineBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(18),
            Padding = new Thickness(18),
            Margin = new Thickness(0, 0, 16, 16),
            Child = stack
        });
    }
    private void SaveProfileButton_Click(object sender, RoutedEventArgs e)
    {
        var name = ProfileNameBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Enter a profile name before saving.", "WinForge", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var path = _service.SaveCurrentProfile(name);
        RefreshSavedProfiles();
        SavedProfilesBox.SelectedItem = System.IO.Path.GetFileNameWithoutExtension(path);
        MessageBox.Show("Profile saved.", "WinForge", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void LoadProfileButton_Click(object sender, RoutedEventArgs e)
    {
        var name = SavedProfilesBox.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Choose a saved profile first.", "WinForge", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (_service.LoadSavedProfile(name))
        {
            MessageBox.Show("Profile loaded. Open Optimise to review and apply the selected options.", "WinForge", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show("The selected profile could not be loaded.", "WinForge", MessageBoxButton.OK, MessageBoxImage.Warning);
            RefreshSavedProfiles();
        }
    }

    private void RefreshSavedProfiles()
    {
        SavedProfilesBox.Items.Clear();

        foreach (var profile in _service.GetSavedProfiles())
        {
            SavedProfilesBox.Items.Add(profile);
        }

        if (SavedProfilesBox.Items.Count > 0)
        {
            SavedProfilesBox.SelectedIndex = 0;
        }
    }

}

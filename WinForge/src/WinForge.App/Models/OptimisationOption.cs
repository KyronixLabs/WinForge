using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WinForge.Models;

public sealed class OptimisationOption : INotifyPropertyChanged
{
    private bool _isSelected;

    public required string Id { get; init; }
    public required string Category { get; init; }
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required string Details { get; init; }
    public required string ApplyScript { get; init; }
    public string? RevertScript { get; init; }
    public bool RequiresAdmin { get; init; } = true;
    public bool HasRevert => !string.IsNullOrWhiteSpace(RevertScript);

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

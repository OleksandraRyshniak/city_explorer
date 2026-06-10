using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CityExplorer.Models;

public class Place : INotifyPropertyChanged
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryEmoji { get; set; } = string.Empty;

    private bool _isFavorite;
    public bool IsFavorite
    {
        get => _isFavorite;
        set
        {
            if (_isFavorite == value) return;
            _isFavorite = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HeartColor));
        }
    }

    // Красное сердечко если избранное, прозрачное если нет
    public Color HeartColor => _isFavorite
        ? Color.FromArgb("#E63946")
        : Colors.Transparent;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
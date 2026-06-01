using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CityExplorer.ViewModels;

/// <summary>
/// Baasklass kõigile ViewModelitele.
/// Sisaldab INotifyPropertyChanged loogikat, et UI uueneks automaatselt.
/// </summary>
public class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T backingStore, T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

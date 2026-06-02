using System.Collections.ObjectModel;
using System.Windows.Input;
using CityExplorer.Models;
using CityExplorer.Resources.Localization;
using CityExplorer.Services;

namespace CityExplorer.ViewModels;

/// <summary>
/// ViewModel 2. vahekaardile "Lemmikud".
/// Laeb SQLite andmebaasist lemmikud ja võimaldab neid kustutada (CRUD - Delete).
/// </summary>
public class FavoritesViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;
    private ObservableCollection<Place> _favorites = new ObservableCollection<Place>();

    public ObservableCollection<Place> Favorites
    {
        get => _favorites;
        set => SetProperty(ref _favorites, value);
    }

    public ICommand LoadCommand { get; }
    public ICommand RemoveCommand { get; }

    public FavoritesViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;

        LoadCommand = new Command(async () => await LoadFavoritesAsync());
        RemoveCommand = new Command<Place>(async (place) => await RemoveFavoriteAsync(place));
    }

    public async Task LoadFavoritesAsync()
    {
        var favorites = await _databaseService.GetFavoritesAsync();
        Favorites = new ObservableCollection<Place>(favorites);
    }

    private async Task RemoveFavoriteAsync(Place place)
    {
        bool confirm = await Application.Current!.MainPage!.DisplayAlert(
            AppResources.DeleteFavorite,
            $"\"{place.Name}\" {AppResources.DeleteConfirm}",
            AppResources.Yes,
            AppResources.No);

        if (!confirm) return;

        await _databaseService.DeletePlaceAsync(place);
        Favorites.Remove(place);
    }
}
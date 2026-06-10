using System.Collections.ObjectModel;
using System.Windows.Input;
using CityExplorer.Models;
using CityExplorer.Resources.Localization;
using CityExplorer.Services;

namespace CityExplorer.ViewModels;

public class ExploreViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;
    private ObservableCollection<Place> _places = [];
    private List<Place> _allLoadedPlaces = [];

    public ObservableCollection<Place> Places
    {
        get => _places;
        set => SetProperty(ref _places, value);
    }

    public ObservableCollection<Category> Categories { get; } =
    [
        new() { Emoji = "🏙️", Title = AppResources.CatAll },
        new() { Emoji = "🏰", Title = AppResources.CatHistory },
        new() { Emoji = "🌳", Title = AppResources.CatParks },
        new() { Emoji = "🍽️", Title = AppResources.CatFood },
        new() { Emoji = "🎭", Title = AppResources.CatCulture },
        new() { Emoji = "⛪", Title = AppResources.CatChurches },
    ];



    public ICommand FilterCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }
    public ICommand AddToFavoritesCommand { get; }
    public ICommand LoadCommand { get; }

    public ExploreViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;

        FilterCommand = new Command<string>(FilterByCategory);
        ToggleFavoriteCommand = new Command<Place>(async (place) => await ToggleFavoriteAsync(place));
        AddToFavoritesCommand = ToggleFavoriteCommand;
        LoadCommand = new Command(async () => await LoadPlacesAsync());
    }

    private void FilterByCategory(string emoji)
    {
        var filtered = emoji == "🏙️"
            ? _allLoadedPlaces
            : _allLoadedPlaces.Where(p => p.CategoryEmoji == emoji).ToList();

        Places = new ObservableCollection<Place>(filtered);
    }

    private async Task ToggleFavoriteAsync(Place place)
    {
        place.IsFavorite = !place.IsFavorite;
        await _databaseService.SavePlaceAsync(place);
        // Принудительно уведомляем UI
        var index = Places.IndexOf(place);
        if (index >= 0)
        {
            Places.RemoveAt(index);
            Places.Insert(index, place);
        }
    }
    public async Task LoadPlacesAsync()
    {
        await _databaseService.InitAsync();

        _allLoadedPlaces = await _databaseService.GetAllPlacesAsync();
        Places = new ObservableCollection<Place>(_allLoadedPlaces);
    }
}
using System.Collections.ObjectModel;
using System.Windows.Input;
using CityExplorer.Models;
using CityExplorer.Resources.Localization;
using CityExplorer.Services;

namespace CityExplorer.ViewModels;

/// <summary>
/// ViewModel 1. vahekaardile "Avasta".
/// Haldab karuselli andmeid ja kategooriate filtreerimist.
/// Staatilised andmed + andmebaasist laadimine.
/// </summary>
public class ExploreViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;
    private ObservableCollection<Place> _places = [];

    public ObservableCollection<Place> Places
    {
        get => _places;
        set => SetProperty(ref _places, value);
    }

    public ObservableCollection<Category> Categories { get; } =
    [
        new() { Emoji = "🏛️", Title = "Kõik" },
        new() { Emoji = "🏰", Title = "Ajalugu" },
        new() { Emoji = "🌳", Title = "Pargid" },
        new() { Emoji = "🍽️", Title = "Toidukohad" },
        new() { Emoji = "🎭", Title = "Kultuur" },
        new() { Emoji = "⛪", Title = "Kirikud" },
    ];

    // Staatilised vaikimisi vaatamisväärsused
    private static readonly List<Place> DefaultPlaces =
    [
        new() { Name = "Raekoja plats",               ImageUrl = "raekoda.jpg",     Description = "Tallinna keskaegne süda, ümbritsetud 13.–14. sajandi hoonetega.",        CategoryEmoji = "🏰" },
        new() { Name = "Toompea loss",                ImageUrl = "toompea.jpg",     Description = "Eesti Riigikogu asukoht ja üks Tallinna olulisimaid vaatamisväärsusi.", CategoryEmoji = "🏰" },
        new() { Name = "Pikk Hermann",                ImageUrl = "pikkhermann.jpg", Description = "Toompea lossi torn, mille tipust lehvib Eesti riigilipp.",              CategoryEmoji = "🏰" },
        new() { Name = "Kadrioru park",               ImageUrl = "kadriorg.jpg",    Description = "Peetri I loodud romantiline barokne park koos Kadrioru paleega.",       CategoryEmoji = "🌳" },
        new() { Name = "Botaanikaaed",                ImageUrl = "botaanika.jpg",   Description = "Üle 8000 taimeliigiga roheline paradiis Pirital.",                      CategoryEmoji = "🌳" },
        new() { Name = "Olde Hansa",                  ImageUrl = "oldehansa.jpg",   Description = "Keskaegse õhkkonnaga restoran vanalinnas, traditsiooniline köök.",      CategoryEmoji = "🍽️" },
        new() { Name = "Leib Resto",                  ImageUrl = "leib.jpg",        Description = "Moodne Eesti köök vanalinnas, hooajalised kohalikud toorained.",        CategoryEmoji = "🍽️" },
        new() { Name = "Kumu kunstimuuseum",          ImageUrl = "kumu.jpg",        Description = "Eesti suurim kunstimuuseum, avatud 2006. aastal Kadriorus.",            CategoryEmoji = "🎭" },
        new() { Name = "Estonia teater",              ImageUrl = "estonia.jpg",     Description = "Eesti rahvusooper ja -ballett, avatud 1913. aastal.",                   CategoryEmoji = "🎭" },
        new() { Name = "Toomkirik",                   ImageUrl = "toomkirik.jpg",   Description = "Tallinna Neitsi Maarja Toomkirik – Eesti vanim kirik.",                CategoryEmoji = "⛪" },
        new() { Name = "Aleksander Nevski katedraal", ImageUrl = "nevski.jpg",      Description = "Hiilgav Vene õigeusu katedraal Toompeal, ehitatud 1900. aastal.",      CategoryEmoji = "⛪" },
    ];

    private List<Place> _allPlaces = new(DefaultPlaces);

    public ICommand FilterCommand { get; }
    public ICommand AddToFavoritesCommand { get; }
    public ICommand LoadCommand { get; }

    public ExploreViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;

        FilterCommand = new Command<string>(FilterByCategory);
        AddToFavoritesCommand = new Command<Place>(async (place) => await AddToFavoritesAsync(place));
        LoadCommand = new Command(async () => await LoadPlacesAsync());
    }

    private void FilterByCategory(string emoji)
    {
        var filtered = emoji == "🏛️"
            ? _allPlaces
            : _allPlaces.Where(p => p.CategoryEmoji == emoji).ToList();

        Places = new ObservableCollection<Place>(filtered);
    }

    private async Task AddToFavoritesAsync(Place place)
    {
        place.IsFavorite = true;
        await _databaseService.SavePlaceAsync(place);

        if (Application.Current?.MainPage is not null)
        {
            await Application.Current.MainPage.DisplayAlert(
                AppResources.AddedToFavorites,
                $"\"{place.Name}\" {AppResources.AddedMessage}",
                AppResources.OK);
        }
    }

    /// <summary>
    /// Laeb nii staatilised kui andmebaasist lisatud kohad.
    /// </summary>
    public async Task LoadPlacesAsync()
    {
        await _databaseService.InitAsync();

        // Laeme andmebaasist kasutaja lisatud kohad
        var dbPlaces = await _databaseService.GetAllPlacesAsync();

        // Ühendame staatilised + andmebaasi kohad (vältides duplikaate nime järgi)
        var combined = new List<Place>(DefaultPlaces);
        foreach (var dbPlace in dbPlaces)
        {
            if (!combined.Any(p => p.Name == dbPlace.Name))
                combined.Add(dbPlace);
        }

        _allPlaces = combined;
        Places = new ObservableCollection<Place>(_allPlaces);
    }
}
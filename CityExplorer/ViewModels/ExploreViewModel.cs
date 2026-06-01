using System.Collections.ObjectModel;
using System.Windows.Input;
using CityExplorer.Models;
using CityExplorer.Services;

namespace CityExplorer.ViewModels;

/// <summary>
/// ViewModel 1. vahekaardile "Avasta".
/// Haldab karuselli andmeid ja kategooriate filtreerimist.
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

    // Kõik vaatamisväärsused (staatilised andmed - laienda vastavalt vajadusele)
    private static readonly List<Place> AllPlaces =
    [
        new() { Name = "Raekoja plats",        ImageUrl = "raekoda.jpg",   Description = "Tallinna keskaegne süda, ümbritsetud 13.–14. sajandi hoonetega.",          CategoryEmoji = "🏰" },
        new() { Name = "Toompea loss",          ImageUrl = "toompea.jpg",   Description = "Eesti Riigikogu asukoht ja üks Tallinna olulisimaid vaatamisväärsusi.",   CategoryEmoji = "🏰" },
        new() { Name = "Pikk Hermann",          ImageUrl = "pikkhermann.jpg",Description = "Toompea lossi torn, mille tipust lehvib Eesti riigilipp.",                CategoryEmoji = "🏰" },
        new() { Name = "Kadrioru park",         ImageUrl = "kadriorg.jpg",  Description = "Peetri I loodud romantiline barokne park koos Kadrioru paleega.",         CategoryEmoji = "🌳" },
        new() { Name = "Botaanikaaed",          ImageUrl = "botaanika.jpg", Description = "Üle 8000 taimeliigiga roheline paradiis Pirital.",                        CategoryEmoji = "🌳" },
        new() { Name = "Olde Hansa",            ImageUrl = "oldehansa.jpg", Description = "Keskaegse õhkkonnaga restoran vanalinnas, traditsiooniline köök.",        CategoryEmoji = "🍽️" },
        new() { Name = "Leib Resto",            ImageUrl = "leib.jpg",      Description = "Moodne Eesti köök vanalinnas, hooajalised kohalikud toorained.",          CategoryEmoji = "🍽️" },
        new() { Name = "Kumu kunstimuuseum",    ImageUrl = "kumu.jpg",      Description = "Eesti suurim kunstimuuseum, avatud 2006. aastal Kadriorus.",              CategoryEmoji = "🎭" },
        new() { Name = "Estonia teater",        ImageUrl = "estonia.jpg",   Description = "Eesti rahvusooper ja -ballett, avatud 1913. aastal.",                     CategoryEmoji = "🎭" },
        new() { Name = "Toomkirik",             ImageUrl = "toomkirik.jpg", Description = "Tallinna Neitsi Maarja Toomkirik – Eesti vanim kirik.",                  CategoryEmoji = "⛪" },
        new() { Name = "Aleksander Nevski katedraal", ImageUrl = "nevski.jpg", Description = "Hiilgav Vene õigeusu katedraal Toompeal, ehitatud 1900. aastal.",    CategoryEmoji = "⛪" },
    ];

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
            ? AllPlaces
            : AllPlaces.Where(p => p.CategoryEmoji == emoji).ToList();

        Places = new ObservableCollection<Place>(filtered);
    }

    private async Task AddToFavoritesAsync(Place place)
    {
        place.IsFavorite = true;
        await _databaseService.SavePlaceAsync(place);

        // Tagasiside kasutajale
        if (Application.Current?.MainPage is not null)
        {
            await Application.Current.MainPage.DisplayAlert(
                "✅ Lisatud!",
                $"\"{place.Name}\" on lemmikutesse lisatud.",
                "OK");
        }
    }

    private async Task LoadPlacesAsync()
    {
        await _databaseService.InitAsync();
        Places = new ObservableCollection<Place>(AllPlaces);
    }
}

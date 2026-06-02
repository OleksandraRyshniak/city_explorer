using System.Collections.ObjectModel;
using System.Windows.Input;
using CityExplorer.Models;
using CityExplorer.Resources.Localization;
using CityExplorer.Services;

namespace CityExplorer.ViewModels;

/// <summary>
/// ViewModel 4. vahekaardile "Admin".
/// Võimaldab lisada uusi kohti andmebaasi ja kustutada olemasolevaid (CRUD - Create, Delete).
/// </summary>
public class AdminViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;

    // ── Vormi väljad ──────────────────────────────────────────
    private string _name = string.Empty;
    private string _description = string.Empty;
    private string _imageUrl = string.Empty;
    private string _selectedCategoryEmoji = "🏰";

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string ImageUrl
    {
        get => _imageUrl;
        set => SetProperty(ref _imageUrl, value);
    }

    public string SelectedCategoryEmoji
    {
        get => _selectedCategoryEmoji;
        set => SetProperty(ref _selectedCategoryEmoji, value);
    }

    // ── Andmed ────────────────────────────────────────────────
    public ObservableCollection<Place> AllPlaces { get; } = [];

    public ObservableCollection<Category> Categories { get; } =
    [
        new() { Emoji = "🏰", Title = "Ajalugu" },
        new() { Emoji = "🌳", Title = "Pargid" },
        new() { Emoji = "🍽️", Title = "Toidukohad" },
        new() { Emoji = "🎭", Title = "Kultuur" },
        new() { Emoji = "⛪", Title = "Kirikud" },
    ];

    // ── Käsud ─────────────────────────────────────────────────
    public ICommand LoadCommand { get; }
    public ICommand SavePlaceCommand { get; }
    public ICommand DeletePlaceCommand { get; }
    public ICommand SelectCategoryCommand { get; }

    public AdminViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;

        LoadCommand = new Command(async () => await LoadAllPlacesAsync());
        SavePlaceCommand = new Command(async () => await SavePlaceAsync());
        DeletePlaceCommand = new Command<Place>(async (place) => await DeletePlaceAsync(place));
        SelectCategoryCommand = new Command<string>(emoji => SelectedCategoryEmoji = emoji);
    }

    private async Task LoadAllPlacesAsync()
    {
        var places = await _databaseService.GetAllPlacesAsync();
        AllPlaces.Clear();
        foreach (var p in places)
            AllPlaces.Add(p);
    }

    private async Task SavePlaceAsync()
    {
        // Valideerimine
        if (string.IsNullOrWhiteSpace(Name) ||
            string.IsNullOrWhiteSpace(Description) ||
            string.IsNullOrWhiteSpace(ImageUrl))
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "❌",
                AppResources.AdminError,
                AppResources.OK);
            return;
        }

        var newPlace = new Place
        {
            Name = Name.Trim(),
            Description = Description.Trim(),
            ImageUrl = ImageUrl.Trim(),
            CategoryEmoji = SelectedCategoryEmoji,
            IsFavorite = false
        };

        await _databaseService.SavePlaceAsync(newPlace);

        // Tühjendame vormi
        Name = string.Empty;
        Description = string.Empty;
        ImageUrl = string.Empty;
        SelectedCategoryEmoji = "🏰";

        // Uuendame nimekirja
        await LoadAllPlacesAsync();

        await Application.Current!.MainPage!.DisplayAlert(
            "✅",
            AppResources.AdminSaved,
            AppResources.OK);
    }

    private async Task DeletePlaceAsync(Place place)
    {
        bool confirm = await Application.Current!.MainPage!.DisplayAlert(
            AppResources.DeleteFavorite,
            $"\"{place.Name}\" {AppResources.DeleteConfirm}",
            AppResources.Yes,
            AppResources.No);

        if (!confirm) return;

        await _databaseService.DeletePlaceAsync(place);
        AllPlaces.Remove(place);
    }
}
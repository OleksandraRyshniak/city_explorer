using CityExplorer.Services;
using CityExplorer.ViewModels;
using CityExplorer.Views;
using Microsoft.Extensions.Logging;

namespace CityExplorer;

/// <summary>
/// Rakenduse peamine konfiguratsioonipunkt.
/// Siin registreeritakse kõik teenused, ViewModelid ja Views DI konteinerisse.
/// </summary>
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ── Teenused (Services) ──────────────────────────────────────────────
        // Singleton: üks eksemplar kogu rakenduse eluea jooksul
        builder.Services.AddSingleton<DatabaseService>();
        

        // ── ViewModels ───────────────────────────────────────────────────────
        builder.Services.AddSingleton<ExploreViewModel>();
        builder.Services.AddSingleton<FavoritesViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();
        builder.Services.AddSingleton<AdminViewModel>();

        // ── Views ────────────────────────────────────────────────────────────
        builder.Services.AddSingleton<ExplorePage>();
        builder.Services.AddSingleton<FavoritesPage>();
        builder.Services.AddSingleton<SettingsPage>();
        builder.Services.AddSingleton<MainTabbedPage>();
        builder.Services.AddSingleton<AdminPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

using CityExplorer.Services;

namespace CityExplorer;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // ✅ Восстанавливаем тему и dark mode при каждом запуске
        ThemeService.RakendaSalvestatud();

        // ✅ Восстанавливаем язык при каждом запуске
        var savedLang = Preferences.Get("app_language", "et");
        var ci = new System.Globalization.CultureInfo(savedLang);
        System.Threading.Thread.CurrentThread.CurrentCulture = ci;
        System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
        CityExplorer.Resources.Localization.AppResources.Culture = ci;

        MainPage = new AppShell();
    }
}
using System.Globalization;
using System.Windows.Input;
using CityExplorer.Resources.Localization;
using CityExplorer.Services;

namespace CityExplorer.ViewModels;

/// <summary>
/// ViewModel 3. vahekaardile "Seaded".
/// Haldab keele vahetamist (.resx failid) ja teema vahetamist (ThemeService).
/// </summary>
public class SettingsViewModel : BaseViewModel
{
    private string _currentLanguage = "et";
    private string _currentTheme = "Navy";

    public string CurrentLanguage
    {
        get => _currentLanguage;
        set => SetProperty(ref _currentLanguage, value);
    }

    public string CurrentTheme
    {
        get => _currentTheme;
        set => SetProperty(ref _currentTheme, value);
    }

    public ICommand ChangeLanguageCommand { get; }
    public ICommand ChangeThemeCommand { get; }

    public SettingsViewModel()
    {
        ChangeLanguageCommand = new Command<string>(ChangeLanguage);
        ChangeThemeCommand = new Command<string>(ChangeTheme);

        // Sünkroniseerime aktiivse teema
        CurrentTheme = ThemeService.CurrentTheme ;
    }

    private void ChangeLanguage(string languageCode)
    {
        CurrentLanguage = languageCode;

        var culture = new CultureInfo(languageCode);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        AppResources.Culture = culture;

        Application.Current?.MainPage?.DisplayAlert(
            AppResources.SettingsTitle,
            AppResources.LanguageChanged,
            AppResources.OK);
    }

    private void ChangeTheme(string themeName)
    {
        CurrentTheme = themeName;
        ThemeService.ApplyTheme(themeName);

        Application.Current?.MainPage?.DisplayAlert(
            AppResources.SettingsTitle,
            AppResources.ThemeChanged,
            AppResources.OK);
    }
}
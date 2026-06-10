using System.Globalization;
using System.Windows.Input;
using CityExplorer.Resources.Localization;
using CityExplorer.Services;

namespace CityExplorer.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private string _currentLanguage;

    public string CurrentLanguage
    {
        get => _currentLanguage;
        set => SetProperty(ref _currentLanguage, value);
    }

    // Dark mode — напрямую через ThemeService
    public bool IsDarkMode
    {
        get => ThemeService.OnTumedTeema;
        set
        {
            ThemeService.OnTumedTeema = value;
            OnPropertyChanged();
        }
    }

    public ICommand ChangeLanguageCommand { get; }

    public SettingsViewModel()
    {
        _currentLanguage = Preferences.Get("app_language", "et");
        ChangeLanguageCommand = new Command<string>(ChangeLanguage);
    }

    private void ChangeLanguage(string languageCode)
    {
        CurrentLanguage = languageCode;

        var culture = new CultureInfo(languageCode);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        AppResources.Culture = culture;
        Preferences.Set("app_language", languageCode);
    }
}
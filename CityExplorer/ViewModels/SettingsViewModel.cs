using System.Globalization;
using System.Windows.Input;
using CityExplorer.Resources.Localization;

namespace CityExplorer.ViewModels;

/// <summary>
/// ViewModel 3. vahekaardile "Seaded".
/// Haldab keele vahetamist .resx failide kaudu.
/// </summary>
public class SettingsViewModel : BaseViewModel
{
    private string _currentLanguage = "et";

    public string CurrentLanguage
    {
        get => _currentLanguage;
        set => SetProperty(ref _currentLanguage, value);
    }

    public ICommand ChangeLanguageCommand { get; }

    public SettingsViewModel()
    {
        ChangeLanguageCommand = new Command<string>(ChangeLanguage);
    }

    private void ChangeLanguage(string languageCode)
    {
        CurrentLanguage = languageCode;

        // Muudame rakenduse keele
        var culture = new CultureInfo(languageCode);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        AppResources.Culture = culture;

        // Teavitame kasutajat
        Application.Current?.MainPage?.DisplayAlert(
            AppResources.SettingsTitle,
            AppResources.LanguageChanged,
            "OK");
    }
}

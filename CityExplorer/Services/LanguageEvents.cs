namespace CityExplorer.Services;

public static class LanguageEvents
{
    public static event Action? LanguageChanged;
    public static void Notify() => LanguageChanged?.Invoke();
}

namespace CityExplorer.Services;

public static class ThemeService
{
    private const string _key = "app_dark_mode";

    public static bool OnTumedTeema
    {
        get => Preferences.Get(_key, false);
        set
        {
            Preferences.Set(_key, value);
            if (Application.Current != null)
                Application.Current.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;
        }
    }

    public static void RakendaSalvestatud()
    {
        if (Application.Current != null)
            Application.Current.UserAppTheme = OnTumedTeema ? AppTheme.Dark : AppTheme.Light;
    }
    // В файле ThemeService.cs — добавьте это свойство в static класс ThemeService
    public static Color Secondary =>
        OnTumedTeema ? Color.FromArgb("#2A2A3E") : Color.FromArgb("#E8E8F4");

    // ── Design tokens ─────────────────────────────────────────────────────────
    public static Color Accent => Color.FromArgb("#E63946");
    public static Color CardLight => Color.FromArgb("#FFFFFF");
    public static Color CardDark => Color.FromArgb("#1C2833");
    public static Color SurfaceLight => Color.FromArgb("#EAF2F8");
    public static Color SurfaceDark => Color.FromArgb("#0D1B2A");
    public static Color TextPrimLight => Color.FromArgb("#1E293B");
    public static Color TextPrimDark => Color.FromArgb("#F1F5F9");
    public static Color TextMutLight => Color.FromArgb("#64748B");
    public static Color TextMutDark => Color.FromArgb("#94A3B8");
}
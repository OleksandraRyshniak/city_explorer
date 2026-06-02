namespace CityExplorer.Services;

/// <summary>
/// Teenus teemade haldamiseks.
/// 4 ilusat teemat: Navy (vaikimisi), Sunset, Forest, Arctic.
/// </summary>
public static class ThemeService
{
    public static string CurrentTheme { get; private set; } = "Navy";

    /// <summary>
    /// Rakendab valitud teema kogu rakendusele.
    /// </summary>
    public static void ApplyTheme(string themeName)
    {
        CurrentTheme = themeName;

        var res = Application.Current!.Resources;

        switch (themeName)
        {
            case "Navy":
                // 🌌 Öine Tallinn — sügav tumesinine + punane
                res["ThemePrimary"] = Color.FromArgb("#0D1B2A");
                res["ThemeAccent"] = Color.FromArgb("#E63946");
                res["ThemeSecondary"] = Color.FromArgb("#1B4F72");
                res["ThemeHeaderBg"] = Color.FromArgb("#0D1B2A");
                res["ThemeButtonBg"] = Color.FromArgb("#1A5276");
                res["ThemeCardBgL"] = Color.FromArgb("#FFFFFF");
                res["ThemeCardBgD"] = Color.FromArgb("#1C2833");
                res["ThemeBgLight"] = Color.FromArgb("#EAF2F8");
                res["ThemeBgDark"] = Color.FromArgb("#0D1B2A");
                break;

            case "Sunset":
                // 🌅 Päikeseloojang — oranž, roosa, lilla
                res["ThemePrimary"] = Color.FromArgb("#2C1654");
                res["ThemeAccent"] = Color.FromArgb("#FF6B35");
                res["ThemeSecondary"] = Color.FromArgb("#C1440E");
                res["ThemeHeaderBg"] = Color.FromArgb("#4A154B");
                res["ThemeButtonBg"] = Color.FromArgb("#C1440E");
                res["ThemeCardBgL"] = Color.FromArgb("#FFF5F0");
                res["ThemeCardBgD"] = Color.FromArgb("#2C1654");
                res["ThemeBgLight"] = Color.FromArgb("#FFF0E8");
                res["ThemeBgDark"] = Color.FromArgb("#1A0A2E");
                break;

            case "Forest":
                // 🌿 Kadrioru Mets — rohelised toonid
                res["ThemePrimary"] = Color.FromArgb("#1B3A2D");
                res["ThemeAccent"] = Color.FromArgb("#52B788");
                res["ThemeSecondary"] = Color.FromArgb("#2D6A4F");
                res["ThemeHeaderBg"] = Color.FromArgb("#1B3A2D");
                res["ThemeButtonBg"] = Color.FromArgb("#2D6A4F");
                res["ThemeCardBgL"] = Color.FromArgb("#F0FFF4");
                res["ThemeCardBgD"] = Color.FromArgb("#1B3A2D");
                res["ThemeBgLight"] = Color.FromArgb("#E8F5E9");
                res["ThemeBgDark"] = Color.FromArgb("#0D1F17");
                break;

            case "Arctic":
                // ❄️ Arktika Jää — külm sinine, valge, hõbedane
                res["ThemePrimary"] = Color.FromArgb("#1A237E");
                res["ThemeAccent"] = Color.FromArgb("#40C4FF");
                res["ThemeSecondary"] = Color.FromArgb("#0277BD");
                res["ThemeHeaderBg"] = Color.FromArgb("#1A237E");
                res["ThemeButtonBg"] = Color.FromArgb("#0277BD");
                res["ThemeCardBgL"] = Color.FromArgb("#E3F2FD");
                res["ThemeCardBgD"] = Color.FromArgb("#0D1B4B");
                res["ThemeBgLight"] = Color.FromArgb("#E8F4FD");
                res["ThemeBgDark"] = Color.FromArgb("#0A0F2E");
                break;
        }
    }

    /// <summary>
    /// Registreerib vaikimisi teema ressursid (käivitatakse MauiProgram.cs-is).
    /// </summary>
    public static void RegisterDefaultResources()
    {
        var res = Application.Current!.Resources;

        // Placeholder väärtused — ApplyTheme kirjutab need üle
        if (!res.ContainsKey("ThemePrimary"))
        {
            res.Add("ThemePrimary", Color.FromArgb("#0D1B2A"));
            res.Add("ThemeAccent", Color.FromArgb("#E63946"));
            res.Add("ThemeSecondary", Color.FromArgb("#1B4F72"));
            res.Add("ThemeHeaderBg", Color.FromArgb("#0D1B2A"));
            res.Add("ThemeButtonBg", Color.FromArgb("#1A5276"));
            res.Add("ThemeCardBgL", Color.FromArgb("#FFFFFF"));
            res.Add("ThemeCardBgD", Color.FromArgb("#1C2833"));
            res.Add("ThemeBgLight", Color.FromArgb("#EAF2F8"));
            res.Add("ThemeBgDark", Color.FromArgb("#0D1B2A"));
        }
    }
}
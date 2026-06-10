using CityExplorer.Resources.Localization;
using CityExplorer.Services;
using CityExplorer.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace CityExplorer.Views;

public partial class SettingsPage : ContentPage
{
    private string _lastBuiltCulture = "";

    public SettingsPage(SettingsViewModel viewModel)
    {
        BindingContext = viewModel;
        Shell.SetNavBarIsVisible(this, false);
        Content = BuildUI();
    }

    public SettingsPage()
    {
        Shell.SetNavBarIsVisible(this, false);
        Content = BuildUI();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LanguageEvents.LanguageChanged += OnLanguageChanged;
        var current = GetCurrentCulture();
        if (current != _lastBuiltCulture)
            ReloadLanguage();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        LanguageEvents.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged() => MainThread.BeginInvokeOnMainThread(ReloadLanguage);

    private static string GetCurrentCulture() =>
        AppResources.Culture?.Name
        ?? System.Threading.Thread.CurrentThread.CurrentUICulture.Name;

    private View BuildUI()
    {
        _lastBuiltCulture = GetCurrentCulture();
        var bg = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            }
        };
        bg.SetAppThemeColor(BackgroundColorProperty,
            Color.FromArgb("#F0F2F8"), Color.FromArgb("#0D1117"));

        // ── Header ───────────────────────────────────────────────────────────
        var header = new Grid
        {
            Padding = new Thickness(20, 52, 20, 24),
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };
        header.SetAppThemeColor(BackgroundColorProperty,
            Color.FromArgb("#1A1A2E"), Color.FromArgb("#0D1117"));

        var titleLabel = new Label
        {
            Text = AppResources.SettingsTitle,
            FontSize = 30,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            VerticalOptions = LayoutOptions.Center
        };
        var gearLabel = new Label
        {
            Text = "⚙️",
            FontSize = 32,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End
        };
        Grid.SetColumn(titleLabel, 0);
        Grid.SetColumn(gearLabel, 1);
        header.Children.Add(titleLabel);
        header.Children.Add(gearLabel);
        Grid.SetRow(header, 0);

        // ── Body ─────────────────────────────────────────────────────────────
        var body = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(16, 20, 16, 32),
                Spacing = 14,
                Children =
                {
                    BuildDarkModeCard(),
                    BuildLanguageCard()
                }
            }
        };
        Grid.SetRow(body, 1);

        bg.Children.Add(header);
        bg.Children.Add(body);
        return bg;
    }

    // ── Dark mode card ────────────────────────────────────────────────────────
    private View BuildDarkModeCard()
    {
        bool isDark = ThemeService.OnTumedTeema;

        var iconLabel = new Label
        {
            Text = isDark ? "🌙" : "☀️",
            FontSize = 28,
            VerticalOptions = LayoutOptions.Center
        };

        var titleLbl = new Label
        {
            Text = AppResources.DarkTheme,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        };
        titleLbl.SetAppThemeColor(Label.TextColorProperty,
            Color.FromArgb("#1A1A2E"), Colors.White);

        var hintLbl = new Label
        {
            Text = AppResources.DarkThemeHint,
            FontSize = 12
        };
        hintLbl.SetAppThemeColor(Label.TextColorProperty,
            Color.FromArgb("#64748B"), Color.FromArgb("#94A3B8"));

        var toggle = new Switch
        {
            IsToggled = isDark,
            VerticalOptions = LayoutOptions.Center
        };
        toggle.SetAppThemeColor(Switch.OnColorProperty,
            ThemeService.Accent, ThemeService.Accent);
        toggle.Toggled += (_, e) =>
        {
            ThemeService.OnTumedTeema = e.Value;
            iconLabel.Text = e.Value ? "🌙" : "☀️";
        };

        var row = new Grid
        {
            ColumnSpacing = 14,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(44) },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };
        row.Add(iconLabel, 0, 0);
        row.Add(new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center,
            Children = { titleLbl, hintLbl } }, 1, 0);
        row.Add(toggle, 2, 0);

        return Card(row, new Thickness(16, 14));
    }

    // ── Language card ─────────────────────────────────────────────────────────
    private View BuildLanguageCard()
    {
        var titleLbl = new Label
        {
            Text = AppResources.Language,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold
        };
        titleLbl.SetAppThemeColor(Label.TextColorProperty,
            Color.FromArgb("#1A1A2E"), Colors.White);

        var hintLbl = new Label
        {
            Text = AppResources.LanguageHint,
            FontSize = 12
        };
        hintLbl.SetAppThemeColor(Label.TextColorProperty,
            Color.FromArgb("#64748B"), Color.FromArgb("#94A3B8"));

        var topRow = new Grid
        {
            ColumnSpacing = 14,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(44) },
                new ColumnDefinition { Width = GridLength.Star }
            }
        };
        var globe = new Label { Text = "🌐", FontSize = 28, VerticalOptions = LayoutOptions.Center };
        topRow.Add(globe, 0, 0);
        topRow.Add(new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center,
            Children = { titleLbl, hintLbl } }, 1, 0);

        string currentLang = Preferences.Get("app_language", "et");

        var flagRow = new HorizontalStackLayout { Spacing = 10, Margin = new Thickness(0, 4, 0, 0) };
        var langs = new[] { ("🇪🇪", "ET", "et"), ("🇷🇺", "RU", "ru"), ("🇬🇧", "EN", "en") };
        foreach (var (flag, code, culture) in langs)
        {
            bool active = currentLang == culture;
            var btn = MakeFlagButton(flag, code, culture, active);
            flagRow.Children.Add(btn);
        }

        var content = new VerticalStackLayout
        {
            Spacing = 12,
            Children = { topRow, new BoxView { HeightRequest = 1,
                Color = Color.FromArgb("#E2E8F0"), Margin = new Thickness(0, 2) }, flagRow }
        };

        return Card(content, new Thickness(16, 14));
    }

    private View MakeFlagButton(string flag, string code, string culture, bool active)
    {
        var flagLbl = new Label
        {
            Text = flag,
            FontSize = 30,
            HorizontalOptions = LayoutOptions.Center
        };
        var codeLbl = new Label
        {
            Text = code,
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            TextColor = active ? ThemeService.Accent : Color.FromArgb("#888888")
        };

        var btn = new Border
        {
            StrokeThickness = active ? 2 : 0,
            Stroke = active ? new SolidColorBrush(ThemeService.Accent) : null,
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            BackgroundColor = active
                ? ThemeService.Accent.WithAlpha(0.10f)
                : Colors.Transparent,
            Padding = new Thickness(14, 10),
            Shadow = active
                ? new Shadow { Brush = new SolidColorBrush(ThemeService.Accent), Opacity = 0.18f, Radius = 8, Offset = new Point(0, 2) }
                : null,
            Content = new VerticalStackLayout
            {
                Spacing = 4,
                HorizontalOptions = LayoutOptions.Center,
                Children = { flagLbl, codeLbl }
            }
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => SwitchLanguage(culture);
        btn.GestureRecognizers.Add(tap);
        return btn;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static Border Card(View content, Thickness padding)
    {
        var b = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Padding = padding,
            Shadow = new Shadow { Brush = Brush.Black, Opacity = 0.07f, Radius = 10, Offset = new Point(0, 2) },
            Content = content
        };
        b.SetAppThemeColor(Border.BackgroundColorProperty,
            ThemeService.CardLight, ThemeService.CardDark);
        return b;
    }

    private void SwitchLanguage(string culture)
    {
        try
        {
            var ci = new System.Globalization.CultureInfo(culture);
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
            AppResources.Culture = ci;
            Preferences.Set("app_language", culture);

            // Уведомляем все страницы о смене языка
            CityExplorer.Services.LanguageEvents.Notify();

            // Обновляем заголовки вкладок
            if (Application.Current?.MainPage is AppShell shell)
                shell.ReloadTabs();

            ReloadLanguage();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Lang] {ex.Message}");
        }
    }

    public void ReloadLanguage()
    {
        Title = AppResources.seaded;
        Content = BuildUI(); // BuildUI() also sets _lastBuiltCulture
    }
}

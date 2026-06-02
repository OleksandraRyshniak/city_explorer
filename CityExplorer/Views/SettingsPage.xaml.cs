using CityExplorer.ViewModels;
using CityExplorer.Resources.Localization;
using Microsoft.Maui.Controls;

namespace CityExplorer.Views
{
    public partial class SettingsPage : ContentPage
    {
        private readonly SettingsViewModel _viewModel;

        // ────────────────────────────────────────────────────────────────
        //  Конструктор с DI
        // ────────────────────────────────────────────────────────────────
        public SettingsPage(SettingsViewModel viewModel)
        {
            _viewModel = viewModel;
            BindingContext = _viewModel;
            BuildUI();
        }

        public SettingsPage()
        {
            BuildUI();
        }

        // ────────────────────────────────────────────────────────────────
        //  Построение UI
        // ────────────────────────────────────────────────────────────────
        private void BuildUI()
        {
            BackgroundColor = AppInfo.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#121212")
                : Color.FromArgb("#F5F5F5");

            // ── Шапка ──────────────────────────────────────────────────
            var header = new StackLayout
            {
                BackgroundColor = Color.FromArgb("#1A1A2E"),
                Padding = new Thickness(16, 48, 16, 16),
                Children =
                {
                    new Label
                    {
                        Text = AppResources.SettingsTitle,
                        FontSize = 28,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    }
                }
            };

            // ── Заголовок «Выбор языка» ─────────────────────────────────
            var langTitleLabel = new Label
            {
                Text = AppResources.SelectLanguage,
                FontSize = 17,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 0, 0, 8)
            };
            langTitleLabel.SetAppThemeColor(Label.TextColorProperty,
                Color.FromArgb("#1A1A2E"), Colors.White);

            // ── Три кнопки языков ───────────────────────────────────────
            var content = new StackLayout
            {
                Padding = new Thickness(24),
                Spacing = 16,
                Children =
                {
                    langTitleLabel,
                    BuildLanguageButton("🇪🇪  Eesti keel", "et"),
                    BuildLanguageButton("🇬🇧  English",    "en"),
                    BuildLanguageButton("🇷🇺  Русский язык", "ru"),
                    BuildCurrentLanguageRow()
                }
            };

            // ── Корневой Grid ───────────────────────────────────────────
            var root = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Star }
                }
            };
            Grid.SetRow(header, 0);
            Grid.SetRow(content, 1);
            root.Children.Add(header);
            root.Children.Add(content);

            Content = root;
        }

        // ────────────────────────────────────────────────────────────────
        //  Вспомогательные методы
        // ────────────────────────────────────────────────────────────────
        private Frame BuildLanguageButton(string text, string langCode)
        {
            var btn = new Button
            {
                Text = text,
                BackgroundColor = Color.FromArgb("#0072CE"),
                TextColor = Colors.White,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 16,
                HeightRequest = 58,
                CommandParameter = langCode
            };
            btn.SetBinding(Button.CommandProperty, nameof(SettingsViewModel.ChangeLanguageCommand));

            return new Frame
            {
                CornerRadius = 16,
                Padding = 0,
                HasShadow = true,
                BorderColor = Colors.Transparent,
                Content = btn
            };
        }

        private StackLayout BuildCurrentLanguageRow()
        {
            var captionLabel = new Label
            {
                Text = "Aktiivne keel:",
                FontSize = 14,
                TextColor = Colors.Gray,
                VerticalOptions = LayoutOptions.Center
            };

            var valueLabel = new Label
            {
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#E63946"),
                VerticalOptions = LayoutOptions.Center
            };
            valueLabel.SetBinding(Label.TextProperty, nameof(SettingsViewModel.CurrentLanguage));

            return new StackLayout
            {
                Margin = new Thickness(0, 16, 0, 0),
                Orientation = StackOrientation.Horizontal,
                Spacing = 8,
                Children = { captionLabel, valueLabel }
            };
        }
    }
}
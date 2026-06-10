using CityExplorer.Models;
using CityExplorer.Resources.Localization;
using CityExplorer.Services;
using CityExplorer.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System.Globalization;

namespace CityExplorer.Views
{
    public class FavCountConverter : IValueConverter
    {
        public object? Convert(object? value, Type t, object? p, CultureInfo c)
        {
            int count = value is System.Collections.ObjectModel.ObservableCollection<Place> list ? list.Count : 0;
            return $"{count} {AppResources.SavedCount}";
        }
        public object? ConvertBack(object? v, Type t, object? p, CultureInfo c)
            => throw new NotImplementedException();
    }

    public partial class FavoritesPage : ContentPage
    {
        private readonly FavoritesViewModel _viewModel;
        private string _lastBuiltCulture = "";

        public FavoritesPage(FavoritesViewModel viewModel)
        {
            _viewModel = viewModel;
            BindingContext = _viewModel;
            Shell.SetNavBarIsVisible(this, false);
            BuildUI();
        }

        public FavoritesPage() { Shell.SetNavBarIsVisible(this, false); BuildUI(); }

        private static string GetCurrentCulture() =>
            CityExplorer.Resources.Localization.AppResources.Culture?.Name
            ?? System.Threading.Thread.CurrentThread.CurrentUICulture.Name;

        private void BuildUI()
        {
            _lastBuiltCulture = GetCurrentCulture();
            this.SetAppThemeColor(BackgroundColorProperty,
                Color.FromArgb("#F0F4FF"), ThemeService.SurfaceDark);

            // ── Header ──────────────────────────────────────────────────────
            var titleLabel = new Label
            {
                Text = AppResources.FavoritesTitle,
                FontSize = 30,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White
            };
            

            var heartDecor = new Label
            {
                Text = "❤️",
                FontSize = 40,
                VerticalOptions = LayoutOptions.Center,
                Opacity = 0.9
            };

            var headerGrid = new Grid
            {
                Padding = new Thickness(20, 52, 20, 24),
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };
            headerGrid.SetAppThemeColor(BackgroundColorProperty,
                Color.FromArgb("#0D1B2A"), Color.FromArgb("#050A10"));

            var titleStack = new VerticalStackLayout { Spacing = 4, Children = { titleLabel } };
            Grid.SetColumn(titleStack, 0);
            Grid.SetColumn(heartDecor, 1);
            headerGrid.Children.Add(titleStack);
            headerGrid.Children.Add(heartDecor);

            // ── Empty view ──────────────────────────────────────────────────
            var emptyView = new VerticalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Spacing = 16,
                Padding = new Thickness(32),
                Children =
                {
                    new Label { Text = "🗺️", FontSize = 72, HorizontalOptions = LayoutOptions.Center },
                    new Label
                    {
                        Text = AppResources.NoFavorites,
                        FontSize = 16,
                        HorizontalTextAlignment = TextAlignment.Center,
                        HorizontalOptions = LayoutOptions.Center,
                        TextColor = Color.FromArgb("#94A3B8")
                    }
                }
            };

            // ── List ────────────────────────────────────────────────────────
            var collection = new CollectionView
            {
                Margin = new Thickness(14, 14, 14, 10),
                EmptyView = emptyView,
                ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 12 },
                ItemTemplate = BuildFavoriteTemplate()
            };
            collection.SetBinding(ItemsView.ItemsSourceProperty, nameof(FavoritesViewModel.Favorites));

            var root = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Star }
                }
            };
            Grid.SetRow(headerGrid, 0);
            Grid.SetRow(collection, 1);
            root.Children.Add(headerGrid);
            root.Children.Add(collection);

            Content = root;
        }

        private DataTemplate BuildFavoriteTemplate()
        {
            return new DataTemplate(() =>
            {
                // Thumbnail with category badge overlay
                var thumb = new Image { Aspect = Aspect.AspectFill, WidthRequest = 110, HeightRequest = 120 };
                thumb.SetBinding(Image.SourceProperty, nameof(Place.ImageUrl));

                var catEmoji = new Label { FontSize = 14 };
                catEmoji.SetBinding(Label.TextProperty, nameof(Place.CategoryEmoji));
                var catBadge = new Border
                {
                    BackgroundColor = Color.FromArgb("#CC0D1B2A"),
                    StrokeShape = new RoundRectangle { CornerRadius = 8 },
                    StrokeThickness = 0,
                    Padding = new Thickness(5, 2),
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.End,
                    Margin = new Thickness(6, 0, 0, 6),
                    Content = catEmoji
                };
                var imgGrid = new Grid();
                imgGrid.Children.Add(thumb);
                imgGrid.Children.Add(catBadge);

                // Name
                var nameLabel = new Label
                {
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 16,
                    MaxLines = 1,
                    LineBreakMode = LineBreakMode.TailTruncation
                };
                nameLabel.SetAppThemeColor(Label.TextColorProperty,
                    Color.FromArgb("#0D1B2A"), Colors.White);
                nameLabel.SetBinding(Label.TextProperty, nameof(Place.Name));

                // Favorite badge
                var favBadge = new Border
                {
                    BackgroundColor = ThemeService.Accent.WithAlpha(0.12f),
                    StrokeShape = new RoundRectangle { CornerRadius = 8 },
                    StrokeThickness = 0,
                    Padding = new Thickness(10, 4),
                    HorizontalOptions = LayoutOptions.Start,
                    Content = new Label
                    {
                        Text = AppResources.FavoriteBadge,
                        FontSize = 11,
                        TextColor = ThemeService.Accent,
                        FontAttributes = FontAttributes.Bold
                    }
                };

                // Details button
                var detailsBtn = new Button
                {
                    Text = AppResources.ReadMore,
                    FontSize = 12,
                    BackgroundColor = Color.FromArgb("#3B82F6"),
                    TextColor = Colors.White,
                    CornerRadius = 10,
                    HeightRequest = 34,
                    HorizontalOptions = LayoutOptions.Start,
                    Padding = new Thickness(14, 0)
                };
                detailsBtn.Clicked += async (sender, _) =>
                {
                    if (sender is Button btn && btn.BindingContext is Place place)
                        await DisplayAlertAsync($"{place.CategoryEmoji} {place.Name}", place.Description, "OK");
                };

                var infoStack = new VerticalStackLayout
                {
                    Padding = new Thickness(14, 12, 8, 12),
                    VerticalOptions = LayoutOptions.Center,
                    Spacing = 7,
                    Children = { nameLabel, favBadge, detailsBtn }
                };

                // Delete button
                var deleteBtn = new Button
                {
                    Text = "🗑️",
                    FontSize = 16,
                    BackgroundColor = ThemeService.Accent.WithAlpha(0.10f),
                    TextColor = ThemeService.Accent,
                    CornerRadius = 12,
                    WidthRequest = 44,
                    HeightRequest = 44,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 0, 12, 0)
                };
                deleteBtn.SetBinding(Button.CommandProperty,
                    new Binding(nameof(FavoritesViewModel.RemoveCommand),
                        source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext,
                            typeof(FavoritesViewModel))));
                deleteBtn.SetBinding(Button.CommandParameterProperty, ".");

                var cardGrid = new Grid
                {
                    HeightRequest = 120,
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(110) },
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = new GridLength(58) }
                    }
                };
                Grid.SetColumn(imgGrid, 0);
                Grid.SetColumn(infoStack, 1);
                Grid.SetColumn(deleteBtn, 2);
                cardGrid.Children.Add(imgGrid);
                cardGrid.Children.Add(infoStack);
                cardGrid.Children.Add(deleteBtn);

                var card = new Border
                {
                    StrokeShape = new RoundRectangle { CornerRadius = 20 },
                    StrokeThickness = 0,
                    Shadow = new Shadow { Brush = Brush.Black, Opacity = 0.08f, Radius = 12, Offset = new Point(0, 3) },
                    Padding = 0,
                    //ClipsToBounds = true,
                    Content = cardGrid
                };
                card.SetAppThemeColor(Border.BackgroundColorProperty,
                    ThemeService.CardLight, ThemeService.CardDark);
                return card;
            });
        }

        // ── Language reload ─────────────────────────────────────────────────
        protected override void OnAppearing()
        {
            base.OnAppearing();
            LanguageEvents.LanguageChanged += OnLanguageChanged;
            if (GetCurrentCulture() != _lastBuiltCulture)
                BuildUI();
            if (_viewModel is null) return;
            _ = _viewModel.LoadFavoritesAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            LanguageEvents.LanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged() => MainThread.BeginInvokeOnMainThread(BuildUI);
    }

    internal static class ViewExtensions
    {
        public static T Also<T>(this T self, Action<T> action) { action(self); return self; }
    }
}

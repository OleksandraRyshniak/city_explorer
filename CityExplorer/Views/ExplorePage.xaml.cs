using CityExplorer.Models;
using CityExplorer.Resources.Localization;
using CityExplorer.Services;
using CityExplorer.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace CityExplorer.Views
{
    public partial class ExplorePage : ContentPage
    {
        private readonly ExploreViewModel _viewModel;
        private CarouselView _carousel = null!;
        private int _position = 0;
        private bool _isPaused = false;
        private string _lastBuiltCulture = "";

        public ExplorePage(ExploreViewModel viewModel)
        {
            _viewModel = viewModel;
            BindingContext = _viewModel;
            Shell.SetNavBarIsVisible(this, false);
            BuildUI();
        }

        public ExplorePage() { Shell.SetNavBarIsVisible(this, false); BuildUI(); }

        private static string GetCurrentCulture() =>
            AppResources.Culture?.Name
            ?? System.Threading.Thread.CurrentThread.CurrentUICulture.Name;

        private void BuildUI()
        {
            _lastBuiltCulture = GetCurrentCulture();
            this.SetAppThemeColor(BackgroundColorProperty,
                Color.FromArgb("#F0F4FF"), ThemeService.SurfaceDark);

            // ── Header ─────────────────────────────────────────────────────
            var titleLbl = new Label
            {
                Text = AppResources.ExploreTitle,
                FontSize = 30,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White
            };
            var subtitleLbl = new Label
            {
                Text = AppResources.CitySubtitle,
                FontSize = 14,
                TextColor = Color.FromArgb("#9DB4D8")
            };

            var header = new VerticalStackLayout
            {
                Padding = new Thickness(20, 52, 20, 20),
                Spacing = 4,
                Children = { titleLbl, subtitleLbl }
            };
            header.SetAppThemeColor(BackgroundColorProperty,
                Color.FromArgb("#0D1B2A"), Color.FromArgb("#050A10"));

            // ── IndicatorView ───────────────────────────────────────────────
            var indicator = new IndicatorView
            {
                IndicatorSize = 8,
                SelectedIndicatorColor = ThemeService.Accent,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 8, 0, 0)
            };
            indicator.SetAppThemeColor(IndicatorView.IndicatorColorProperty,
                Color.FromArgb("#CBD5E1"), Color.FromArgb("#334155"));

            // ── CarouselView ────────────────────────────────────────────────
            _carousel = new CarouselView
            {
                IndicatorView = indicator,
                Loop = true,
                HeightRequest = 380,
                PeekAreaInsets = new Thickness(20)
            };
            _carousel.SetBinding(ItemsView.ItemsSourceProperty, nameof(ExploreViewModel.Places));
            _carousel.ItemTemplate = BuildCarouselTemplate();

            Dispatcher.StartTimer(TimeSpan.FromSeconds(4), () =>
            {
                if (_carousel?.ItemsSource is not System.Collections.IList list || list.Count == 0)
                    return true;
                if (!_isPaused)
                {
                    _position = (_position + 1) % list.Count;
                    _carousel.Position = _position;
                }
                return true;
            });

            // ── Category sidebar ────────────────────────────────────────────
            var catCollection = new CollectionView
            {
                ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 8 },
                ItemTemplate = BuildCategoryTemplate()
            };
            catCollection.SetBinding(ItemsView.ItemsSourceProperty, nameof(ExploreViewModel.Categories));

            var catScroll = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                VerticalScrollBarVisibility = ScrollBarVisibility.Never,
                Content = new VerticalStackLayout
                {
                    Padding = new Thickness(6, 10),
                    Spacing = 0,
                    Children = { catCollection }
                }
            };

            var contentGrid = new Grid
            {
                Margin = new Thickness(0, 10, 0, 0),
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = new GridLength(66) }
                }
            };
            var carouselStack = new StackLayout { Children = { _carousel, indicator } };
            Grid.SetColumn(carouselStack, 0);
            Grid.SetColumn(catScroll, 1);
            contentGrid.Children.Add(carouselStack);
            contentGrid.Children.Add(catScroll);

            var root = new Grid
            {
                Padding = new Thickness(0, 0, 0, 10),
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Star }
                }
            };
            Grid.SetRow(header, 0);
            Grid.SetRow(contentGrid, 1);
            root.Children.Add(header);
            root.Children.Add(contentGrid);

            Content = root;
        }

        private DataTemplate BuildCarouselTemplate()
        {
            return new DataTemplate(() =>
            {
                // Full-bleed image
                var image = new Image { Aspect = Aspect.AspectFill };
                image.SetBinding(Image.SourceProperty, nameof(Place.ImageUrl));

                // Gradient overlay (bottom)
                var gradient = new BoxView
                {
                    Background = new LinearGradientBrush(
                        new GradientStopCollection
                        {
                            new GradientStop(Colors.Transparent, 0f),
                            new GradientStop(Color.FromArgb("#E0000000"), 1f)
                        },
                        new Point(0, 0), new Point(0, 1))
                };

                // Category badge — top left
                var emojiLabel = new Label { FontSize = 20 };
                emojiLabel.SetBinding(Label.TextProperty, nameof(Place.CategoryEmoji));
                var emojiBadge = new Border
                {
                    BackgroundColor = Color.FromArgb("#CC0D1B2A"),
                    StrokeShape = new RoundRectangle { CornerRadius = 12 },
                    StrokeThickness = 0,
                    Padding = new Thickness(8, 5),
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Start,
                    Margin = new Thickness(12, 12, 0, 0),
                    Content = emojiLabel
                };

                // Heart — top right
                var heartFilled = new Label
                {
                    Text = "❤️", FontSize = 24,
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Start,
                    Margin = new Thickness(0, 12, 14, 0)
                };
                heartFilled.SetBinding(Label.IsVisibleProperty, nameof(Place.IsFavorite));
                var heartOutline = new Label
                {
                    Text = "🤍", FontSize = 24,
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Start,
                    Margin = new Thickness(0, 12, 14, 0)
                };

                var heartTap = new TapGestureRecognizer();
                heartTap.SetBinding(TapGestureRecognizer.CommandProperty,
                    new Binding(nameof(ExploreViewModel.ToggleFavoriteCommand),
                        source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext,
                            typeof(ExploreViewModel))));
                heartTap.SetBinding(TapGestureRecognizer.CommandParameterProperty, ".");

                var heartGrid = new Grid
                {
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Start
                };
                heartGrid.Children.Add(heartOutline);
                heartGrid.Children.Add(heartFilled);
                heartGrid.GestureRecognizers.Add(heartTap);

                // Bottom overlay — name + button
                var nameLabel = new Label
                {
                    FontSize = 20,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White,
                    MaxLines = 1,
                    LineBreakMode = LineBreakMode.TailTruncation
                };
                nameLabel.SetBinding(Label.TextProperty, nameof(Place.Name));

                var detailsBtn = new Button
                {
                    Text = AppResources.ReadMore,
                    FontSize = 13,
                    BackgroundColor = ThemeService.Accent,
                    TextColor = Colors.White,
                    CornerRadius = 10,
                    HeightRequest = 38,
                    HorizontalOptions = LayoutOptions.Start,
                    Padding = new Thickness(18, 0)
                };
                detailsBtn.Clicked += async (sender, _) =>
                {
                    if (sender is Button btn && btn.BindingContext is Place place)
                    {
                        _isPaused = true;
                        await DisplayAlertAsync($"{place.CategoryEmoji} {place.Name}", place.Description, "OK");
                        _isPaused = false;
                    }
                };

                var bottomOverlay = new VerticalStackLayout
                {
                    Padding = new Thickness(16, 12, 16, 18),
                    Spacing = 10,
                    VerticalOptions = LayoutOptions.End,
                    Children = { nameLabel, detailsBtn }
                };

                // Compose card layers
                var cardContent = new Grid();
                cardContent.Children.Add(image);
                cardContent.Children.Add(gradient);
                cardContent.Children.Add(emojiBadge);
                cardContent.Children.Add(heartGrid);
                cardContent.Children.Add(bottomOverlay);

                var card = new Border
                {
                    Margin = new Thickness(8, 4),
                    StrokeShape = new RoundRectangle { CornerRadius = 22 },
                    StrokeThickness = 0,
                    //ClipsToBounds = true,
                    Shadow = new Shadow { Brush = Brush.Black, Opacity = 0.18f, Radius = 18, Offset = new Point(0, 5) },
                    Padding = 0,
                    Content = cardContent
                };
                card.SetAppThemeColor(Border.BackgroundColorProperty,
                    ThemeService.CardLight, ThemeService.CardDark);
                return card;
            });
        }

        private DataTemplate BuildCategoryTemplate()
        {
            return new DataTemplate(() =>
            {
                var emojiLabel = new Label
                {
                    FontSize = 24,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };
                emojiLabel.SetBinding(Label.TextProperty, nameof(Category.Emoji));

                var card = new Border
                {
                    StrokeShape = new RoundRectangle { CornerRadius = 16 },
                    StrokeThickness = 0,
                    WidthRequest = 54,
                    HeightRequest = 54,
                    Shadow = new Shadow { Brush = Brush.Black, Opacity = 0.08f, Radius = 8, Offset = new Point(0, 2) },
                    Content = emojiLabel
                };
                card.SetAppThemeColor(Border.BackgroundColorProperty,
                    ThemeService.CardLight, ThemeService.CardDark);

                var tap = new TapGestureRecognizer();
                tap.SetBinding(TapGestureRecognizer.CommandProperty,
                    new Binding(nameof(ExploreViewModel.FilterCommand),
                        source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext,
                            typeof(ExploreViewModel))));
                tap.SetBinding(TapGestureRecognizer.CommandParameterProperty, nameof(Category.Emoji));
                card.GestureRecognizers.Add(tap);
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
            _ = _viewModel.LoadPlacesAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            LanguageEvents.LanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged() => MainThread.BeginInvokeOnMainThread(BuildUI);
    }
}

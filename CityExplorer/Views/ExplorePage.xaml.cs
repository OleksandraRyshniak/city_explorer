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
        private bool _timerStarted = false;
        private string _lastBuiltCulture = "";

        public ExplorePage(ExploreViewModel viewModel)
        {
            _viewModel = viewModel;
            BindingContext = _viewModel;
            Shell.SetNavBarIsVisible(this, false);
            BuildUI();
        }

        public ExplorePage()
        {
            Shell.SetNavBarIsVisible(this, false);
            BuildUI();
        }

        private static string GetCurrentCulture() =>
            AppResources.Culture?.Name
            ?? System.Threading.Thread.CurrentThread.CurrentUICulture.Name;

        private void BuildUI()
        {
            _lastBuiltCulture = GetCurrentCulture();
            this.SetAppThemeColor(BackgroundColorProperty,
                Color.FromArgb("#F0F4FF"), ThemeService.SurfaceDark);

            // ── Header ──────────────────────────────────────────────
            var titleLbl = new Label
            {
                Text = AppResources.ExploreTitle,
                FontSize = 28,
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
                Padding = new Thickness(20, 48, 20, 16),
                Spacing = 4,
                Children = { titleLbl, subtitleLbl }
            };
            header.SetAppThemeColor(BackgroundColorProperty,
                Color.FromArgb("#0D1B2A"), Color.FromArgb("#050A10"));

            // ── Категории: горизонтальная строка ────────────────────
            var catCollection = new CollectionView
            {
                ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
                {
                    ItemSpacing = 10
                },
                ItemTemplate = BuildCategoryTemplate(),
                HeightRequest = 64,
                Margin = new Thickness(16, 12, 16, 0),
                HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
                MaximumWidthRequest = 480,
                HorizontalOptions = LayoutOptions.Center
            };
            catCollection.SetBinding(ItemsView.ItemsSourceProperty,
                nameof(ExploreViewModel.Categories));

            // ── IndicatorView ───────────────────────────────────────
            var indicator = new IndicatorView
            {
                IndicatorSize = 8,
                SelectedIndicatorColor = ThemeService.Accent,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 6, 0, 0)
            };
            indicator.SetAppThemeColor(IndicatorView.IndicatorColorProperty,
                Color.FromArgb("#CBD5E1"), Color.FromArgb("#334155"));

            // ── CarouselView ────────────────────────────────────────
            _carousel = new CarouselView
            {
                IndicatorView = indicator,
                Loop = false,
                HeightRequest = 400,
                WidthRequest =300,
                PeekAreaInsets = new Thickness(0),
                Margin = new Thickness(0, 4, 0, 0),
                MaximumWidthRequest = 480,
                HorizontalOptions = LayoutOptions.Center
            };
            _carousel.SetBinding(ItemsView.ItemsSourceProperty,
                nameof(ExploreViewModel.Places));
            _carousel.ItemTemplate = BuildCarouselTemplate();

            
            _carousel.PositionChanged += (_, e) => _position = e.CurrentPosition;

            if (!_timerStarted)
            {
                _timerStarted = true;
                Dispatcher.StartTimer(TimeSpan.FromSeconds(4), () =>
                {
                    if (_carousel?.ItemsSource is not System.Collections.IList list || list.Count == 0)
                        return true;
                    if (!_isPaused && !_carousel.IsDragging)   // ← пауза пока пользователь тянет пальцем
                    {
                        _position = (_position + 1) % list.Count;
                        _carousel.ScrollTo(_position,
                            position: ScrollToPosition.Center, animate: true);
                    }
                    return true;
                });
            }
            // ── Корневой layout ─────────────────────────────────────
            var root = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto }, // header
                    new RowDefinition { Height = GridLength.Auto }, // категории
                    new RowDefinition { Height = GridLength.Star }, // карусель
                    new RowDefinition { Height = GridLength.Auto }  // индикатор
                },
                Padding = new Thickness(0, 0, 0, 12)
            };
            root.Add(header, 0, 0);
            root.Add(catCollection, 0, 1);
            root.Add(_carousel, 0, 2);
            root.Add(indicator, 0, 3);

            Content = root;
        }

        // ── Шаблон карточки карусели ────────────────────────────────
        private DataTemplate BuildCarouselTemplate()
        {
            return new DataTemplate(() =>
            {
                var image = new Image { Aspect = Aspect.AspectFill };
                image.SetBinding(Image.SourceProperty, nameof(Place.ImageUrl));

                // Градиент снизу — чтобы текст читался
                var gradient = new BoxView
                {
                    Background = new LinearGradientBrush(
                        new GradientStopCollection
                        {
                            new GradientStop(Colors.Transparent, 0.4f),
                            new GradientStop(Color.FromArgb("#D9000000"), 1f)
                        },
                        new Point(0, 0), new Point(0, 1)),
                    InputTransparent = true
                };

                // Бейдж категории — слева сверху
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

                // Сердечко — справа сверху
                var heartFilled = new Label { Text = "❤️", FontSize = 24 };
                heartFilled.SetBinding(Label.IsVisibleProperty, nameof(Place.IsFavorite));
                var heartOutline = new Label { Text = "🤍", FontSize = 24 };

                var heartTap = new TapGestureRecognizer();
                heartTap.SetBinding(TapGestureRecognizer.CommandProperty,
                    new Binding(nameof(ExploreViewModel.ToggleFavoriteCommand),
                        source: new RelativeBindingSource(
                            RelativeBindingSourceMode.FindAncestorBindingContext,
                            typeof(ExploreViewModel))));
                heartTap.SetBinding(TapGestureRecognizer.CommandParameterProperty, ".");

                var heartGrid = new Grid
                {
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Start,
                    Margin = new Thickness(0, 12, 14, 0),
                    Padding = new Thickness(6) // увеличивает зону нажатия
                };
                heartGrid.Children.Add(heartOutline);
                heartGrid.Children.Add(heartFilled);
                heartGrid.GestureRecognizers.Add(heartTap);

                // Название + кнопка — снизу
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
                        await DisplayAlert($"{place.CategoryEmoji} {place.Name}",
                            place.Description, "OK");
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

                // Слои карточки
                var cardContent = new Grid();
                cardContent.Children.Add(image);
                cardContent.Children.Add(gradient);
                cardContent.Children.Add(emojiBadge);
                cardContent.Children.Add(heartGrid);
                cardContent.Children.Add(bottomOverlay);

                var card = new Border
                {
                    Margin = new Thickness(16, 4),
                    StrokeShape = new RoundRectangle { CornerRadius = 22 },
                    StrokeThickness = 0,
                    // Если картинка не загрузится — тёмный фон вместо серого
                    BackgroundColor = Color.FromArgb("#1B2A3D"),
                    Shadow = new Shadow
                    {
                        Brush = Brush.Black,
                        Opacity = 0.18f,
                        Radius = 14,
                        Offset = new Point(0, 4)
                    },
                    Padding = 0,
                    Content = cardContent
                };
                return card;
            });
        }

        // ── Шаблон категории ────────────────────────────────────────
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
                    Content = emojiLabel
                };
                card.SetAppThemeColor(Border.BackgroundColorProperty,
                    ThemeService.CardLight, ThemeService.CardDark);

                var tap = new TapGestureRecognizer();
                tap.SetBinding(TapGestureRecognizer.CommandProperty,
                    new Binding(nameof(ExploreViewModel.FilterCommand),
                        source: new RelativeBindingSource(
                            RelativeBindingSourceMode.FindAncestorBindingContext,
                            typeof(ExploreViewModel))));
                tap.SetBinding(TapGestureRecognizer.CommandParameterProperty,
                    nameof(Category.Emoji));
                card.GestureRecognizers.Add(tap);
                return card;
            });
        }

        // ── Перезагрузка языка ──────────────────────────────────────
      
            protected override void OnAppearing()
        {
            base.OnAppearing();
            LanguageEvents.LanguageChanged += OnLanguageChanged;
            if (GetCurrentCulture() != _lastBuiltCulture)
                BuildUI();
            if (_viewModel is null) return;
            _ = _viewModel.LoadPlacesAsync();


            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(300), () =>
            {
                if (_carousel?.ItemsSource is System.Collections.IList list && list.Count > 0)
                {
                    if (_position >= list.Count) _position = 0;
                    _carousel.ScrollTo(_position,
                        position: ScrollToPosition.Start, animate: false);
                }
            });
        }
        

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            LanguageEvents.LanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged() =>
            MainThread.BeginInvokeOnMainThread(BuildUI);
    }
}
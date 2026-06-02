using CityExplorer.Models;
using CityExplorer.Resources.Localization;
using CityExplorer.ViewModels;
using Microsoft.Maui.Controls;


namespace CityExplorer.Views
{
    public partial class ExplorePage : ContentPage
    {
        private readonly ExploreViewModel _viewModel;

        // ────────────────────────────────────────────────────────────────
        //  Конструктор – DI через параметр (MauiProgram.cs регистрирует)
        // ────────────────────────────────────────────────────────────────
        public ExplorePage(ExploreViewModel viewModel)
        {
            _viewModel = viewModel;
            BindingContext = _viewModel;
            BuildUI();
        }

        // Пустой конструктор для обратной совместимости
        public ExplorePage()
        {
            BuildUI();
        }

        // ────────────────────────────────────────────────────────────────
        //  Построение всего UI программно
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
                        Text = AppResources.ExploreTitle,
                        FontSize = 28,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    },
                    new Label
                    {
                        Text = "🇪🇪 Tallinn",
                        FontSize = 14,
                        TextColor = Color.FromArgb("#AAAAAA")
                    }
                }
            };

            // ── IndicatorView (объявляем раньше CarouselView) ───────────
            var indicator = new IndicatorView
            {
                IndicatorColor = Color.FromArgb("#CCCCCC"),
                SelectedIndicatorColor = Color.FromArgb("#E63946"),
                IndicatorSize = 8,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 6, 0, 0)
            };

            // ── CarouselView ────────────────────────────────────────────
            var carousel = new CarouselView
            {
                IndicatorView = indicator,
                Loop = true,
                HeightRequest = 400,
                PeekAreaInsets = new Thickness(16)
            };
            carousel.SetBinding(ItemsView.ItemsSourceProperty, nameof(ExploreViewModel.Places));
            carousel.ItemTemplate = BuildCarouselTemplate();

            var carouselStack = new StackLayout
            {
                Children = { carousel, indicator }
            };

            // ── Боковое меню категорий ──────────────────────────────────
            var categoriesCollection = new CollectionView
            {
                ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 6 }
            };
            categoriesCollection.SetBinding(ItemsView.ItemsSourceProperty, nameof(ExploreViewModel.Categories));
            categoriesCollection.ItemTemplate = BuildCategoryTemplate();

            var categoryScroll = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                VerticalScrollBarVisibility = ScrollBarVisibility.Never,
                Content = new VerticalStackLayout
                {
                    Padding = new Thickness(4, 8),
                    Spacing = 6,
                    Children = { categoriesCollection }
                }
            };

            // ── Основная сетка (контент) ────────────────────────────────
            var contentGrid = new Grid
            {
                Margin = new Thickness(0, 8, 0, 0),
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = new GridLength(64) }
                }
            };
            Grid.SetColumn(carouselStack, 0);
            Grid.SetColumn(categoryScroll, 1);
            contentGrid.Children.Add(carouselStack);
            contentGrid.Children.Add(categoryScroll);

            // ── Корневой Grid ───────────────────────────────────────────
            var root = new Grid
            {
                Padding = new Thickness(0, 0, 0, 8),
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

        // ────────────────────────────────────────────────────────────────
        //  DataTemplate для карточки карусели
        // ────────────────────────────────────────────────────────────────
        private DataTemplate BuildCarouselTemplate()
        {
            return new DataTemplate(() =>
            {
                // Изображение
                var image = new Image { Aspect = Aspect.AspectFill, HeightRequest = 220 };
                image.SetBinding(Image.SourceProperty, nameof(Place.ImageUrl));

                // Значок категории поверх изображения
                var emojiLabel = new Label { FontSize = 18 };
                emojiLabel.SetBinding(Label.TextProperty, nameof(Place.CategoryEmoji));
                var emojiBadge = new Frame
                {
                    BackgroundColor = Color.FromArgb("#CC000000"),
                    CornerRadius = 12,
                    Padding = new Thickness(6, 3),
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Start,
                    Margin = new Thickness(10),
                    HasShadow = false,
                    Content = emojiLabel
                };

                // Название
                var nameLabel = new Label
                {
                    FontSize = 18,
                    FontAttributes = FontAttributes.Bold
                };
                nameLabel.SetAppThemeColor(Label.TextColorProperty,
                    Color.FromArgb("#1A1A2E"), Colors.White);
                nameLabel.SetBinding(Label.TextProperty, nameof(Place.Name));

                // Описание
                var descLabel = new Label
                {
                    FontSize = 13,
                    TextColor = Colors.Gray,
                    MaxLines = 2,
                    LineBreakMode = LineBreakMode.TailTruncation
                };
                descLabel.SetBinding(Label.TextProperty, nameof(Place.Description));

                // Кнопка «Добавить в избранное»
                var addBtn = new Button
                {
                    Text = AppResources.AddToFavorites,
                    BackgroundColor = Color.FromArgb("#E63946"),
                    TextColor = Colors.White,
                    CornerRadius = 10,
                    FontSize = 13,
                    HeightRequest = 40,
                    Margin = new Thickness(0, 6, 0, 0)
                };
                addBtn.SetBinding(Button.CommandProperty,
                    new Binding(nameof(ExploreViewModel.AddToFavoritesCommand),
                        source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext,
                            typeof(ExploreViewModel))));
                addBtn.SetBinding(Button.CommandParameterProperty, ".");

                var infoStack = new StackLayout
                {
                    Padding = new Thickness(16, 12, 16, 8),
                    Spacing = 4,
                    Children = { nameLabel, descLabel, addBtn }
                };

                var innerGrid = new Grid
                {
                    RowDefinitions =
                    {
                        new RowDefinition { Height = new GridLength(220) },
                        new RowDefinition { Height = GridLength.Star }
                    }
                };
                Grid.SetRow(image, 0);
                Grid.SetRow(emojiBadge, 0);
                Grid.SetRow(infoStack, 1);
                innerGrid.Children.Add(image);
                innerGrid.Children.Add(emojiBadge);
                innerGrid.Children.Add(infoStack);

                return new Frame
                {
                    Margin = new Thickness(8, 4),
                    CornerRadius = 20,
                    HasShadow = true,
                    Padding = 0,
                    BorderColor = Colors.Transparent,
                    Content = innerGrid
                };
            });
        }

        // ────────────────────────────────────────────────────────────────
        //  DataTemplate для кнопки-эмодзи категории
        // ────────────────────────────────────────────────────────────────
        private DataTemplate BuildCategoryTemplate()
        {
            return new DataTemplate(() =>
            {
                var emojiLabel = new Label
                {
                    FontSize = 26,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };
                emojiLabel.SetBinding(Label.TextProperty, nameof(Category.Emoji));

                var frame = new Frame
                {
                    CornerRadius = 14,
                    Padding = 0,
                    HasShadow = false,
                    WidthRequest = 52,
                    HeightRequest = 52,
                    Content = emojiLabel
                };
                frame.SetAppThemeColor(VisualElement.BackgroundColorProperty,
                    Colors.White, Color.FromArgb("#2A2A2A"));

                var tap = new TapGestureRecognizer();
                tap.SetBinding(TapGestureRecognizer.CommandProperty,
                    new Binding(nameof(ExploreViewModel.FilterCommand),
                        source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext,
                            typeof(ExploreViewModel))));
                // ✅ Исправлено: строковый путь "Emoji" передаёт значение свойства,
                //    а не его имя как литерал (что делал nameof)
                tap.SetBinding(TapGestureRecognizer.CommandParameterProperty, "Emoji");
                frame.GestureRecognizers.Add(tap);

                return frame;
            });
        }

        // ────────────────────────────────────────────────────────────────
        //  Lifecycle – загрузка данных при появлении страницы
        // ────────────────────────────────────────────────────────────────
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel != null)
                // ✅ Исправлено: убрано бессмысленное присвоение `object valuesult =`
                await _viewModel.LoadPlacesAsync();
        }
    }
}
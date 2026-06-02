using CityExplorer.Models;
using CityExplorer.ViewModels;
using CityExplorer.Resources.Localization;
using Microsoft.Maui.Controls;

namespace CityExplorer.Views
{
    public partial class FavoritesPage : ContentPage
    {
        private readonly FavoritesViewModel _viewModel;

        // ────────────────────────────────────────────────────────────────
        //  Конструктор с DI
        // ────────────────────────────────────────────────────────────────
        public FavoritesPage(FavoritesViewModel viewModel)
        {
            _viewModel = viewModel;
            BindingContext = _viewModel;
            BuildUI();
        }

        public FavoritesPage()
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
                        Text = AppResources.FavoritesTitle,
                        FontSize = 28,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    }
                }
            };

            // ── Заглушка «список пуст» ──────────────────────────────────
            var emptyView = new StackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Spacing = 12,
                Padding = new Thickness(32),
                Children =
                {
                    new Label
                    {
                        Text = "❤️",
                        FontSize = 56,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    new Label
                    {
                        Text = AppResources.NoFavorites,
                        FontSize = 16,
                        TextColor = Colors.Gray,
                        HorizontalTextAlignment = TextAlignment.Center,
                        HorizontalOptions = LayoutOptions.Center
                    }
                }
            };

            // ── Список избранного ───────────────────────────────────────
            var collection = new CollectionView
            {
                Margin = new Thickness(8),
                EmptyView = emptyView,
                ItemTemplate = BuildFavoriteTemplate()
            };
            collection.SetBinding(ItemsView.ItemsSourceProperty, nameof(FavoritesViewModel.Favorites));

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
            Grid.SetRow(collection, 1);
            root.Children.Add(header);
            root.Children.Add(collection);

            Content = root;
        }

        // ────────────────────────────────────────────────────────────────
        //  DataTemplate для одной карточки избранного
        // ────────────────────────────────────────────────────────────────
        private DataTemplate BuildFavoriteTemplate()
        {
            return new DataTemplate(() =>
            {
                // Миниатюра
                var thumb = new Image { Aspect = Aspect.AspectFill, WidthRequest = 100 };
                thumb.SetBinding(Image.SourceProperty, nameof(Place.ImageUrl));

                // Название
                var nameLabel = new Label
                {
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 15,
                    MaxLines = 1,
                    LineBreakMode = LineBreakMode.TailTruncation
                };
                nameLabel.SetAppThemeColor(Label.TextColorProperty,
                    Color.FromArgb("#1A1A2E"), Colors.White);
                nameLabel.SetBinding(Label.TextProperty, nameof(Place.Name));

                // Эмодзи категории
                var catLabel = new Label { FontSize = 20 };
                catLabel.SetBinding(Label.TextProperty, nameof(Place.CategoryEmoji));

                // Краткое описание
                var descLabel = new Label
                {
                    FontSize = 11,
                    TextColor = Colors.Gray,
                    MaxLines = 1,
                    LineBreakMode = LineBreakMode.TailTruncation
                };
                descLabel.SetBinding(Label.TextProperty, nameof(Place.Description));

                var infoStack = new StackLayout
                {
                    Padding = new Thickness(12, 10),
                    VerticalOptions = LayoutOptions.Center,
                    Spacing = 4,
                    Children = { nameLabel, catLabel, descLabel }
                };

                // Кнопка «Удалить»
                var deleteBtn = new Button
                {
                    Text = "🗑️",
                    FontSize = 20,
                    BackgroundColor = Colors.Transparent,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                };
                deleteBtn.SetBinding(Button.CommandProperty,
                    new Binding(nameof(FavoritesViewModel.RemoveCommand),
                        source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext,
                            typeof(FavoritesViewModel))));
                deleteBtn.SetBinding(Button.CommandParameterProperty, ".");

                // Внутренняя сетка карточки
                var cardGrid = new Grid
                {
                    HeightRequest = 96,
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(100) },
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = new GridLength(56) }
                    }
                };
                Grid.SetColumn(thumb, 0);
                Grid.SetColumn(infoStack, 1);
                Grid.SetColumn(deleteBtn, 2);
                cardGrid.Children.Add(thumb);
                cardGrid.Children.Add(infoStack);
                cardGrid.Children.Add(deleteBtn);

                return new Frame
                {
                    Margin = new Thickness(4),
                    CornerRadius = 14,
                    HasShadow = true,
                    Padding = 0,
                    BorderColor = Colors.Transparent,
                    Content = cardGrid
                };
            });
        }

        // ────────────────────────────────────────────────────────────────
        //  Lifecycle – перезагрузка при каждом появлении вкладки
        // ────────────────────────────────────────────────────────────────
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel != null)
                await _viewModel.LoadFavoritesAsync();
        }
    }
}
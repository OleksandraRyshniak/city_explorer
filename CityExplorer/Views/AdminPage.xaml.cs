using CityExplorer.Models;
using CityExplorer.Services;
using Microsoft.Maui.Controls;

namespace CityExplorer.Views
{
    /// <summary>
    /// Страница администратора: полный CRUD для управления
    /// достопримечательностями (Place) в базе данных SQLite.
    ///
    /// Функции:
    ///   • Список всех мест (CollectionView)
    ///   • Форма добавления нового места
    ///   • Редактирование существующего места
    ///   • Удаление записи с подтверждением
    ///   • Счётчик записей в шапке
    /// </summary>
    public partial class AdminPage : ContentPage
    {
        // ────────────────────────────────────────────────────────────────
        //  Поля
        // ────────────────────────────────────────────────────────────────
        private readonly DatabaseService _db;

        // Ссылки на элементы формы (нужны при сохранении)
        private Entry _nameEntry = null!;
        private Entry _descEntry = null!;
        private Entry _imageEntry = null!;
        private Entry _emojiEntry = null!;
        private Picker _categoryPicker = null!;
        private Button _saveBtn = null!;
        private Label _counterLabel = null!;
        private CollectionView _listView = null!;

        // Текущий редактируемый объект (null = создание нового)
        private Place? _editingPlace = null;

        // ────────────────────────────────────────────────────────────────
        //  Конструктор с DI
        // ────────────────────────────────────────────────────────────────
        public AdminPage(DatabaseService db)
        {
            _db = db;
            BuildUI();
        }

        public AdminPage()
        {
            // Без DI – для превью / обратная совместимость
            BuildUI();
        }

        // ════════════════════════════════════════════════════════════════
        //  ПОСТРОЕНИЕ UI
        // ════════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            BackgroundColor = AppInfo.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#121212")
                : Color.FromArgb("#F5F5F5");

            Content = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto }, // шапка
                    new RowDefinition { Height = GridLength.Auto }, // форма
                    new RowDefinition { Height = GridLength.Star }  // список
                },
                Children =
                {
                    BuildHeader(),
                    BuildForm(),
                    BuildList()
                }
            };

            // Устанавливаем строки Grid
            var root = (Grid)Content;
            Grid.SetRow((View)root.Children[0], 0);
            Grid.SetRow((View)root.Children[1], 1);
            Grid.SetRow((View)root.Children[2], 2);
        }

        // ────────────────────────────────────────────────────────────────
        //  Шапка
        // ────────────────────────────────────────────────────────────────
        private View BuildHeader()
        {
            _counterLabel = new Label
            {
                Text = "Kohti andmebaasis: 0",
                FontSize = 13,
                TextColor = Color.FromArgb("#AAAAAA")
            };

            return new StackLayout
            {
                BackgroundColor = Color.FromArgb("#1A1A2E"),
                Padding = new Thickness(16, 48, 16, 16),
                Children =
                {
                    new Label
                    {
                        Text = "⚙️ Admin",
                        FontSize = 28,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    },
                    _counterLabel
                }
            };
        }

        // ────────────────────────────────────────────────────────────────
        //  Форма добавления / редактирования
        // ────────────────────────────────────────────────────────────────
        private View BuildForm()
        {
            // ── Поля ввода ──────────────────────────────────────────────
            _nameEntry = new Entry
            {
                Placeholder = "Koha nimi (nt Raekoda)",
                ClearButtonVisibility = ClearButtonVisibility.WhileEditing
            };

            _descEntry = new Entry
            {
                Placeholder = "Lühikirjeldus",
                ClearButtonVisibility = ClearButtonVisibility.WhileEditing
            };

            _imageEntry = new Entry
            {
                Placeholder = "Pildi URL või nimi (nt raekoda.jpg)",
                ClearButtonVisibility = ClearButtonVisibility.WhileEditing,
                Keyboard = Keyboard.Url
            };

            _emojiEntry = new Entry
            {
                Placeholder = "Kategooria emotikonid (nt 🏰)",
                ClearButtonVisibility = ClearButtonVisibility.WhileEditing,
                MaxLength = 4
            };

            _categoryPicker = new Picker
            {
                Title = "Vali kategooria"
            };
            _categoryPicker.Items.Add("🏰 Ajalugu");
            _categoryPicker.Items.Add("🌳 Pargid");
            _categoryPicker.Items.Add("🍽️ Toidukohad");
            _categoryPicker.Items.Add("🎭 Kultuur");
            _categoryPicker.Items.Add("🛍️ Ostlemine");
            _categoryPicker.SelectedIndexChanged += OnCategoryPickerChanged;

            // ── Кнопки ──────────────────────────────────────────────────
            _saveBtn = new Button
            {
                Text = "➕ Lisa koht",
                BackgroundColor = Color.FromArgb("#2ECC71"),
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 12,
                HeightRequest = 50
            };
            _saveBtn.Clicked += OnSaveClicked;

            var clearBtn = new Button
            {
                Text = "🔄 Tühjenda",
                BackgroundColor = Color.FromArgb("#888888"),
                TextColor = Colors.White,
                CornerRadius = 12,
                HeightRequest = 50
            };
            clearBtn.Clicked += (_, _) => ClearForm();

            var btnRow = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = new GridLength(130) }
                },
                ColumnSpacing = 8
            };
            Grid.SetColumn(_saveBtn, 0);
            Grid.SetColumn(clearBtn, 1);
            btnRow.Children.Add(_saveBtn);
            btnRow.Children.Add(clearBtn);

            // ── Обёртка формы ───────────────────────────────────────────
            var formFrame = new Frame
            {
                Margin = new Thickness(12, 10),
                Padding = new Thickness(16),
                CornerRadius = 16,
                HasShadow = true,
                BorderColor = Colors.Transparent,
                Content = new StackLayout
                {
                    Spacing = 10,
                    Children =
                    {
                        new Label
                        {
                            Text = "Uue koha lisamine",
                            FontSize = 16,
                            FontAttributes = FontAttributes.Bold
                        },
                        _nameEntry,
                        _descEntry,
                        _imageEntry,
                        _emojiEntry,
                        _categoryPicker,
                        btnRow
                    }
                }
            };

            return formFrame;
        }

        // ────────────────────────────────────────────────────────────────
        //  Список мест
        // ────────────────────────────────────────────────────────────────
        private View BuildList()
        {
            _listView = new CollectionView
            {
                Margin = new Thickness(8, 0, 8, 8),
                EmptyView = BuildEmptyView(),
                ItemTemplate = BuildPlaceTemplate()
            };

            return new ScrollView { Content = _listView };
        }

        private View BuildEmptyView()
        {
            return new StackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Spacing = 8,
                Padding = new Thickness(32),
                Children =
                {
                    new Label { Text = "🗺️", FontSize = 48, HorizontalOptions = LayoutOptions.Center },
                    new Label
                    {
                        Text = "Andmebaas on tühi. Lisa esimene koht üleval oleva vormi abil.",
                        FontSize = 14,
                        TextColor = Colors.Gray,
                        HorizontalTextAlignment = TextAlignment.Center
                    }
                }
            };
        }

        // ────────────────────────────────────────────────────────────────
        //  DataTemplate для строки списка
        // ────────────────────────────────────────────────────────────────
        private DataTemplate BuildPlaceTemplate()
        {
            return new DataTemplate(() =>
            {
                // Миниатюра
                var img = new Image { Aspect = Aspect.AspectFill, WidthRequest = 80 };
                img.SetBinding(Image.SourceProperty, nameof(Place.ImageUrl));

                // Эмодзи
                var emoji = new Label { FontSize = 22, VerticalOptions = LayoutOptions.Center };
                emoji.SetBinding(Label.TextProperty, nameof(Place.CategoryEmoji));

                // Название
                var name = new Label { FontAttributes = FontAttributes.Bold, FontSize = 14, MaxLines = 1, LineBreakMode = LineBreakMode.TailTruncation };
                name.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#1A1A2E"), Colors.White);
                name.SetBinding(Label.TextProperty, nameof(Place.Name));

                // Описание
                var desc = new Label { FontSize = 11, TextColor = Colors.Gray, MaxLines = 1, LineBreakMode = LineBreakMode.TailTruncation };
                desc.SetBinding(Label.TextProperty, nameof(Place.Description));

                var infoStack = new StackLayout
                {
                    Padding = new Thickness(8, 6),
                    VerticalOptions = LayoutOptions.Center,
                    Spacing = 2,
                    Children = { emoji, name, desc }
                };

                // Кнопка «Редактировать»
                var editBtn = new Button
                {
                    Text = "✏️",
                    FontSize = 18,
                    BackgroundColor = Color.FromArgb("#3498DB"),
                    TextColor = Colors.White,
                    CornerRadius = 8,
                    WidthRequest = 44,
                    HeightRequest = 44,
                    VerticalOptions = LayoutOptions.Center
                };
                editBtn.SetBinding(Button.CommandParameterProperty, ".");
                editBtn.Clicked += async (sender, _) =>
                {
                    if (sender is Button btn && btn.CommandParameter is Place place)
                        await LoadPlaceForEdit(place);
                };

                // Кнопка «Удалить»
                var delBtn = new Button
                {
                    Text = "🗑️",
                    FontSize = 18,
                    BackgroundColor = Color.FromArgb("#E74C3C"),
                    TextColor = Colors.White,
                    CornerRadius = 8,
                    WidthRequest = 44,
                    HeightRequest = 44,
                    VerticalOptions = LayoutOptions.Center
                };
                delBtn.SetBinding(Button.CommandParameterProperty, ".");
                delBtn.Clicked += async (sender, _) =>
                {
                    if (sender is Button btn && btn.CommandParameter is Place place)
                        await DeletePlaceAsync(place);
                };

                var actionStack = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Spacing = 4,
                    Padding = new Thickness(4),
                    VerticalOptions = LayoutOptions.Center,
                    Children = { editBtn, delBtn }
                };

                var cardGrid = new Grid
                {
                    HeightRequest = 88,
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(80) },
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Auto }
                    }
                };
                Grid.SetColumn(img, 0);
                Grid.SetColumn(infoStack, 1);
                Grid.SetColumn(actionStack, 2);
                cardGrid.Children.Add(img);
                cardGrid.Children.Add(infoStack);
                cardGrid.Children.Add(actionStack);

                return new Frame
                {
                    Margin = new Thickness(0, 3),
                    CornerRadius = 12,
                    HasShadow = false,
                    Padding = 0,
                    BorderColor = Colors.Transparent,
                    Content = cardGrid
                };
            });
        }

        // ════════════════════════════════════════════════════════════════
        //  ЛОГИКА
        // ════════════════════════════════════════════════════════════════

        // ────────────────────────────────────────────────────────────────
        //  Подбор эмодзи при выборе категории в Picker
        // ────────────────────────────────────────────────────────────────
        private void OnCategoryPickerChanged(object? sender, EventArgs e)
        {
            if (_categoryPicker.SelectedIndex < 0) return;

            string[] emojis = { "🏰", "🌳", "🍽️", "🎭", "🛍️" };
            _emojiEntry.Text = emojis[_categoryPicker.SelectedIndex];
        }

        // ────────────────────────────────────────────────────────────────
        //  Сохранение (создание или обновление)
        // ────────────────────────────────────────────────────────────────
        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(_nameEntry.Text))
            {
                await DisplayAlert("Viga", "Palun sisesta koha nimi.", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(_descEntry.Text))
            {
                await DisplayAlert("Viga", "Palun sisesta lühikirjeldus.", "OK");
                return;
            }

            if (_db == null)
            {
                await DisplayAlert("Info", "DatabaseService pole ühendatud (prevüü režiim).", "OK");
                return;
            }

            if (_editingPlace == null)
            {
                // ── Создание нового места ───────────────────────────────
                var newPlace = new Place
                {
                    Name = _nameEntry.Text.Trim(),
                    Description = _descEntry.Text.Trim(),
                    ImageUrl = _imageEntry.Text?.Trim() ?? "placeholder.jpg",
                    CategoryEmoji = _emojiEntry.Text?.Trim() ?? "📍",
                    IsFavorite = false
                };
                await _db.SavePlaceAsync(newPlace);
                await DisplayAlert("✅ Lisatud", $"«{newPlace.Name}» on andmebaasi salvestatud.", "OK");
            }
            else
            {
                // ── Обновление существующего места ──────────────────────
                _editingPlace.Name = _nameEntry.Text.Trim();
                _editingPlace.Description = _descEntry.Text.Trim();
                _editingPlace.ImageUrl = _imageEntry.Text?.Trim() ?? _editingPlace.ImageUrl;
                _editingPlace.CategoryEmoji = _emojiEntry.Text?.Trim() ?? _editingPlace.CategoryEmoji;
                await _db.SavePlaceAsync(_editingPlace);
                await DisplayAlert("✅ Uuendatud", $"«{_editingPlace.Name}» andmed on uuendatud.", "OK");
                _editingPlace = null;
                _saveBtn.Text = "➕ Lisa koht";
                _saveBtn.BackgroundColor = Color.FromArgb("#2ECC71");
            }

            ClearForm();
            await RefreshListAsync();
        }

        // ────────────────────────────────────────────────────────────────
        //  Загрузка места в форму для редактирования
        // ────────────────────────────────────────────────────────────────
        private async Task LoadPlaceForEdit(Place place)
        {
            _editingPlace = place;
            _nameEntry.Text = place.Name;
            _descEntry.Text = place.Description;
            _imageEntry.Text = place.ImageUrl;
            _emojiEntry.Text = place.CategoryEmoji;

            _saveBtn.Text = "💾 Salvesta muutused";
            _saveBtn.BackgroundColor = Color.FromArgb("#E67E22");

            await DisplayAlert("Muutmine", $"Muudad kohta: «{place.Name}»", "OK");

            // Прокрутить форму вверх
            if (Content is ScrollView sv) await sv.ScrollToAsync(0, 0, true);
        }

        // ────────────────────────────────────────────────────────────────
        //  Удаление с подтверждением
        // ────────────────────────────────────────────────────────────────
        private async Task DeletePlaceAsync(Place place)
        {
            bool confirm = await DisplayAlert(
                "Kustuta koht",
                $"Kas soovid kustutada «{place.Name}»?",
                "Jah, kustuta",
                "Tühista");

            if (!confirm) return;

            if (_db == null)
            {
                await DisplayAlert("Info", "DatabaseService pole ühendatud.", "OK");
                return;
            }

            await _db.DeletePlaceAsync(place);
            await RefreshListAsync();
        }

        // ────────────────────────────────────────────────────────────────
        //  Обновление списка и счётчика
        // ────────────────────────────────────────────────────────────────
        private async Task RefreshListAsync()
        {
            if (_db == null) return;

            var places = await _db.GetAllPlacesAsync();
            _listView.ItemsSource = null;
            _listView.ItemsSource = places;
            _counterLabel.Text = $"Kohti andmebaasis: {places.Count}";
        }

        // ────────────────────────────────────────────────────────────────
        //  Очистка формы
        // ────────────────────────────────────────────────────────────────
        private void ClearForm()
        {
            _nameEntry.Text = string.Empty;
            _descEntry.Text = string.Empty;
            _imageEntry.Text = string.Empty;
            _emojiEntry.Text = string.Empty;
            _categoryPicker.SelectedIndex = -1;
            _editingPlace = null;
            _saveBtn.Text = "➕ Lisa koht";
            _saveBtn.BackgroundColor = Color.FromArgb("#2ECC71");
        }

        // ────────────────────────────────────────────────────────────────
        //  Lifecycle
        // ────────────────────────────────────────────────────────────────
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await RefreshListAsync();
        }
    }
}
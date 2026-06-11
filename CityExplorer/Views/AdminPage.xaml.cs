using CityExplorer.Models;
using CityExplorer.Resources.Localization;
using CityExplorer.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;

namespace CityExplorer.Views
{
    public partial class AdminPage : ContentPage
    {
        private readonly DatabaseService _db;

        private Entry _nameEntry = null!;
        private Entry _descEntry = null!;
        private Entry _imageEntry = null!;
        private Picker _categoryPicker = null!;
        private Button _saveBtn = null!;
        private Label _counterLabel = null!;
        private VerticalStackLayout _listContainer = null!;
        private Place? _editingPlace = null;

        // Form state snapshot for rebuilds
        private string _savedName = "";
        private string _savedDesc = "";
        private string _savedImage = "";
        private int _savedCatIdx = -1;
        private string _lastBuiltCulture = "";

        private static readonly string[] CategoryEmojis = { "🏰", "🌳", "🍽️", "🎭", "🛍️", "⛪" };
        private static string[] CategoryNames =>
        [
            AppResources.CatHistory,
            AppResources.CatParks,
            AppResources.CatFood,
            AppResources.CatCulture,
            AppResources.CatShopping,
            AppResources.CatChurches
        ];

        public AdminPage(DatabaseService db) { _db = db; Shell.SetNavBarIsVisible(this, false); BuildUI(); }
        public AdminPage() { Shell.SetNavBarIsVisible(this, false); BuildUI(); }

        private static string GetCurrentCulture() =>
            AppResources.Culture?.Name
            ?? System.Threading.Thread.CurrentThread.CurrentUICulture.Name;

        private void BuildUI()
        {
            

            // ── Header ──────────────────────────────────────────────────
            _counterLabel = new Label { FontSize = 13, TextColor = Color.FromArgb("#AAAAAA") };

            var header = new Grid
            {
                Padding = new Thickness(20, 52, 20, 20),
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };
            header.SetAppThemeColor(BackgroundColorProperty,
                Color.FromArgb("#1A1A2E"), Color.FromArgb("#0D1117"));

            var titleCol = new VerticalStackLayout
            {
                Spacing = 4,
                Children =
                {
                    new Label
                    {
                        Text = AppResources.AdminTitle,
                        FontSize = 28,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    },
                    _counterLabel
                }
            };
            var gearDecor = new Label
            {
                Text = "⚙️",
                FontSize = 36,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End
            };
            Grid.SetColumn(titleCol, 0);
            Grid.SetColumn(gearDecor, 1);
            header.Children.Add(titleCol);
            header.Children.Add(gearDecor);

            // ── Buttons ─────────────────────────────────────────────────
            _saveBtn = new Button
            {
                Text = $"💾  {AppResources.AddNewPlace}",
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                FontSize = 16,
                HeightRequest = 54,
                CornerRadius = 16,
                BackgroundColor = Color.FromArgb("#2ECC71"),
                Margin = new Thickness(0, 8, 0, 4)
            };
            _saveBtn.Clicked += OnSaveClicked;

            var clearBtn = new Button
            {
                Text = $"🔄  {AppResources.ClearForm}",
                TextColor = Colors.White,
                FontSize = 14,
                HeightRequest = 44,
                CornerRadius = 12,
                BackgroundColor = Color.FromArgb("#888888"),
                Margin = new Thickness(0, 0, 0, 8)
            };
            clearBtn.Clicked += (_, _) => ClearForm();

            _listContainer = new VerticalStackLayout { Spacing = 10 };

            var scrollContent = new VerticalStackLayout
            {
                Spacing = 12,
                Padding = new Thickness(16, 12, 16, 24),
                Children =
                {
                    BuildHeaderCard(),
                    BuildNameCard(),
                    BuildImageCard(),
                    BuildCategoryCard(),
                    _saveBtn,
                    clearBtn,
                    BuildListHeader(),
                    _listContainer
                }
            };

            // Restore form state after rebuild
            if (_nameEntry != null && !string.IsNullOrEmpty(_savedName))
            {
                _nameEntry.Text = _savedName;
                _descEntry.Text = _savedDesc;
                _imageEntry.Text = _savedImage;
                _categoryPicker.SelectedIndex = _savedCatIdx;
                if (_editingPlace != null)
                {
                    _saveBtn.Text = $"💾  {AppResources.SaveChanges}";
                    _saveBtn.BackgroundColor = Color.FromArgb("#E67E22");
                }
            }

            var root = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Star }
                }
            };
            Grid.SetRow(header, 0);
            var scroll = new ScrollView { Content = scrollContent };
            Grid.SetRow(scroll, 1);
            root.Children.Add(header);
            root.Children.Add(scroll);

            Content = root;
        }

        private View BuildHeaderCard()
        {
            var lbl = new Label
            {
                Text = $"🏙️ {AppResources.AddNewPlace}",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold
            };
            lbl.SetAppThemeColor(Label.TextColorProperty, ThemeService.TextPrimLight, ThemeService.TextPrimDark);
            return SectionBorder(new VerticalStackLayout { Children = { lbl } });
        }

        private View BuildNameCard()
        {
            _nameEntry = StyledEntry(AppResources.PlaceName);
            _descEntry = StyledEntry(AppResources.ShortDescription);
            return LabelledCard($"📝  {AppResources.NameDescLabel}",
                new VerticalStackLayout { Spacing = 10, Children = { _nameEntry, _descEntry } });
        }

        private View BuildImageCard()
        {
            _imageEntry = StyledEntry(AppResources.ImageUrlHint);
            _imageEntry.Keyboard = Keyboard.Url;

            var preview = new Image { HeightRequest = 180, Aspect = Aspect.AspectFill, IsVisible = false };
            _imageEntry.TextChanged += (_, e) =>
            {
                var txt = e.NewTextValue?.Trim() ?? "";
                preview.IsVisible = !string.IsNullOrEmpty(txt);
                if (preview.IsVisible) preview.Source = txt;
            };

            var galleryBtn = new Button
            {
                Text = $"📷  {AppResources.FromGallery}",
                BackgroundColor = ThemeService.Secondary,
                TextColor = Colors.White,
                CornerRadius = 12,
                FontSize = 14,
                HeightRequest = 44
            };
            galleryBtn.Clicked += async (_, _) =>
            {
                try
                {
                    var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions { Title = AppResources.FromGallery });
                    if (result is null) return;
                    var dest = System.IO.Path.Combine(FileSystem.AppDataDirectory, result.FileName);
                    using var src = await result.OpenReadAsync();
                    using var file = File.OpenWrite(dest);
                    await src.CopyToAsync(file);
                    _imageEntry.Text = dest;
                    preview.Source = dest;
                    preview.IsVisible = true;
                }
                catch (Exception ex) { await DisplayAlertAsync("⚠️", ex.Message, "OK"); }
            };

            return LabelledCard($"🖼️  {AppResources.PhotoLabel}",
                new VerticalStackLayout { Spacing = 10, Children = { preview, galleryBtn, _imageEntry } });
        }

        private View BuildCategoryCard()
        {
            var names = CategoryNames;

            _categoryPicker = new Picker
            {
                Title = AppResources.SelectCategory,
                FontSize = 17,                    // было 15
                HeightRequest = 58,               // было 48
                HorizontalOptions = LayoutOptions.Fill
            };
            _categoryPicker.SetAppThemeColor(Picker.TextColorProperty,
                ThemeService.TextPrimLight, ThemeService.TextPrimDark);
            _categoryPicker.SetAppThemeColor(Picker.TitleColorProperty,
                ThemeService.TextMutLight, ThemeService.TextMutDark);
            foreach (var name in names)
                _categoryPicker.Items.Add(name);

            // Рамка вокруг Picker — выглядит крупнее и заметнее
            var pickerBorder = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                StrokeThickness = 1.5,
                Padding = new Thickness(14, 2),
                Content = _categoryPicker
            };
            pickerBorder.SetAppThemeColor(Border.StrokeProperty,
                Color.FromArgb("#C5CAE9"), Color.FromArgb("#3A3A55"));
            pickerBorder.SetAppThemeColor(Border.BackgroundColorProperty,
                Color.FromArgb("#F5F6FF"), Color.FromArgb("#1E1E30"));

            var chipsLayout = new FlexLayout
            {
                Wrap = FlexWrap.Wrap,
                JustifyContent = FlexJustify.Start,
                AlignItems = FlexAlignItems.Center,
                Direction = FlexDirection.Row
            };

            for (int i = 0; i < CategoryEmojis.Length; i++)
            {
                int idx = i;
                var chipLabel = new Label
                {
                    Text = $"{CategoryEmojis[i]} {names[i]}",
                    FontSize = 15                  
                };
                chipLabel.SetAppThemeColor(Label.TextColorProperty,
                    ThemeService.TextPrimLight, ThemeService.TextPrimDark);

                var chip = new Border
                {
                    StrokeShape = new RoundRectangle { CornerRadius = 14 },
                    StrokeThickness = 0,
                    Padding = new Thickness(16, 11),   
                    Margin = new Thickness(5),
                    Content = chipLabel
                };
                chip.SetAppThemeColor(Border.BackgroundColorProperty,
                    Color.FromArgb("#E8E8F4"), Color.FromArgb("#2A2A3E"));

                var tap = new TapGestureRecognizer();
                tap.Tapped += (_, _) => _categoryPicker.SelectedIndex = idx;
                chip.GestureRecognizers.Add(tap);
                chipsLayout.Add(chip);
            }

            return LabelledCard($"🏷️  {AppResources.CategoryLabel}",
                new VerticalStackLayout { Spacing = 14, Children = { chipsLayout, pickerBorder } });
        }

        private View BuildListHeader()
        {
            var lbl = new Label
            {
                Text = $"📋  {AppResources.AllPlaces}",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 4, 0, 0)
            };
            lbl.SetAppThemeColor(Label.TextColorProperty, ThemeService.TextPrimLight, ThemeService.TextPrimDark);
            return lbl;
        }

        private void RebuildList(List<Place> places)
        {
            _listContainer.Children.Clear();
            foreach (var place in places)
                _listContainer.Children.Add(BuildPlaceRow(place));

            if (places.Count == 0)
            {
                var empty = new Label
                {
                    Text = $"🗺️ {AppResources.EmptyDatabase}",
                    FontSize = 14,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 24)
                };
                empty.SetAppThemeColor(Label.TextColorProperty, ThemeService.TextMutLight, ThemeService.TextMutDark);
                _listContainer.Children.Add(empty);
            }
        }

        private View BuildPlaceRow(Place place)
        {
            var img = new Image { Aspect = Aspect.AspectFill, WidthRequest = 80, HeightRequest = 80 };
            img.Source = place.ImageUrl;

            var name = new Label
            {
                Text = place.Name,
                FontAttributes = FontAttributes.Bold,
                FontSize = 15,
                MaxLines = 1,
                LineBreakMode = LineBreakMode.TailTruncation
            };
            name.SetAppThemeColor(Label.TextColorProperty, ThemeService.TextPrimLight, ThemeService.TextPrimDark);

            var desc = new Label
            {
                Text = place.Description,
                FontSize = 12,
                MaxLines = 1,
                LineBreakMode = LineBreakMode.TailTruncation
            };
            desc.SetAppThemeColor(Label.TextColorProperty, ThemeService.TextMutLight, ThemeService.TextMutDark);

            var infoStack = new StackLayout
            {
                Spacing = 3,
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(10, 6),
                Children = { new Label { Text = place.CategoryEmoji, FontSize = 18 }, name, desc }
            };

            var editBtn = new Button
            {
                Text = "✏️",
                FontSize = 10,
                BackgroundColor = Color.FromArgb("#3498DB"),
                TextColor = Colors.White,
                CornerRadius = 10,
                WidthRequest = 45,
                HeightRequest = 40,
                VerticalOptions = LayoutOptions.Center
            };
            editBtn.Clicked += async (_, _) => await LoadPlaceForEdit(place);

            var delBtn = new Button
            {
                Text = "🗑️",
                FontSize = 10,
                BackgroundColor = ThemeService.Accent,
                TextColor = Colors.White,
                CornerRadius = 10,
                WidthRequest = 45,
                HeightRequest = 40,
                VerticalOptions = LayoutOptions.Center
            };
            delBtn.Clicked += async (_, _) => await DeletePlaceAsync(place);

            var btnStack = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Spacing = 6,
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(4),
                Children = { editBtn, delBtn }
            };

            var cardGrid = new Grid
            {
                HeightRequest = 90,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(80) },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = new GridLength(52) }
                }
            };
            Grid.SetColumn(img, 0);
            Grid.SetColumn(infoStack, 1);
            Grid.SetColumn(btnStack, 2);
            cardGrid.Children.Add(img);
            cardGrid.Children.Add(infoStack);
            cardGrid.Children.Add(btnStack);

            var card = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 16 },
                StrokeThickness = 0,
                Shadow = new Shadow { Brush = Brush.Black, Opacity = 0.07f, Radius = 8, Offset = new Point(0, 2) },
                Padding = 0,
                
                Content = cardGrid
            };
            card.SetAppThemeColor(Border.BackgroundColorProperty, ThemeService.CardLight, ThemeService.CardDark);
            return card;
        }

        // ── Logic ──────────────────────────────────────────────────────
        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_nameEntry.Text))
            { await DisplayAlertAsync("⚠️", AppResources.ValidationNameMissing, "OK"); return; }
            if (string.IsNullOrWhiteSpace(_descEntry.Text))
            { await DisplayAlertAsync("⚠️", AppResources.ValidationDescMissing, "OK"); return; }
            if (_db == null) return;

            int catIdx = _categoryPicker.SelectedIndex;
            string emoji = catIdx >= 0 ? CategoryEmojis[catIdx] : "📍";

            if (_editingPlace == null)
            {
                var p = new Place
                {
                    Name = _nameEntry.Text.Trim(),
                    Description = _descEntry.Text.Trim(),
                    ImageUrl = _imageEntry.Text?.Trim() ?? "placeholder.jpg",
                    CategoryEmoji = emoji,
                    IsFavorite = false
                };
                await _db.SavePlaceAsync(p);
                await DisplayAlertAsync("✅", $"{AppResources.PlaceAdded}: «{p.Name}»", "OK");
            }
            else
            {
                _editingPlace.Name = _nameEntry.Text.Trim();
                _editingPlace.Description = _descEntry.Text.Trim();
                _editingPlace.ImageUrl = _imageEntry.Text?.Trim() ?? _editingPlace.ImageUrl;
                _editingPlace.CategoryEmoji = emoji;
                await _db.SavePlaceAsync(_editingPlace);
                await DisplayAlertAsync("✅", $"{AppResources.PlaceUpdated}: «{_editingPlace.Name}»", "OK");
                _editingPlace = null;
                _saveBtn.Text = $"💾  {AppResources.AddNewPlace}";
                _saveBtn.BackgroundColor = Color.FromArgb("#2ECC71");
            }

            ClearForm();
            await RefreshListAsync();
        }

        private async Task LoadPlaceForEdit(Place place)
        {
            _editingPlace = place;
            _nameEntry.Text = place.Name;
            _descEntry.Text = place.Description;
            _imageEntry.Text = place.ImageUrl;
            var emojiIdx = Array.IndexOf(CategoryEmojis, place.CategoryEmoji);
            _categoryPicker.SelectedIndex = emojiIdx >= 0 ? emojiIdx : -1;
            _saveBtn.Text = $"💾  {AppResources.SaveChanges}";
            _saveBtn.BackgroundColor = Color.FromArgb("#E67E22");
            await DisplayAlertAsync(AppResources.EditPlace, $"{AppResources.EditingPlace}: «{place.Name}»", "OK");
        }

        private async Task DeletePlaceAsync(Place place)
        {
            bool ok = await DisplayAlertAsync(AppResources.DeletePlace,
                $"{AppResources.DeletePlaceConfirm} «{place.Name}»?", AppResources.Yes, AppResources.No);
            if (!ok || _db == null) return;
            await _db.DeletePlaceAsync(place);
            await RefreshListAsync();
        }

        private async Task RefreshListAsync()
        {
            if (_db == null) return;
            var places = await _db.GetAllPlacesAsync();
            _counterLabel.Text = $"{AppResources.PlacesInDb}: {places.Count}";
            RebuildList(places);
        }

        private void ClearForm()
        {
            _nameEntry.Text = string.Empty;
            _descEntry.Text = string.Empty;
            _imageEntry.Text = string.Empty;
            _categoryPicker.SelectedIndex = -1;
            _editingPlace = null;
            _savedName = _savedDesc = _savedImage = "";
            _savedCatIdx = -1;
            _saveBtn.Text = $"💾  {AppResources.AddNewPlace}";
            _saveBtn.BackgroundColor = Color.FromArgb("#2ECC71");
        }

        // ── Language reload ─────────────────────────────────────────────
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            LanguageEvents.LanguageChanged += OnLanguageChanged;
            // Rebuild if language changed while on another tab
            if (GetCurrentCulture() != _lastBuiltCulture)
            {
                _savedName = _nameEntry?.Text ?? "";
                _savedDesc = _descEntry?.Text ?? "";
                _savedImage = _imageEntry?.Text ?? "";
                _savedCatIdx = _categoryPicker?.SelectedIndex ?? -1;
                BuildUI();
            }
            await RefreshListAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            LanguageEvents.LanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Save form state before rebuild
                _savedName = _nameEntry?.Text ?? "";
                _savedDesc = _descEntry?.Text ?? "";
                _savedImage = _imageEntry?.Text ?? "";
                _savedCatIdx = _categoryPicker?.SelectedIndex ?? -1;
                BuildUI();
                _ = RefreshListAsync();
            });
        }

        // ── Helpers ────────────────────────────────────────────────────
        private static Border LabelledCard(string title, View content)
        {
            var header = new Label
            {
                Text = title,
                FontAttributes = FontAttributes.Bold,
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 8)
            };
            header.SetAppThemeColor(Label.TextColorProperty,
                ThemeService.TextMutLight, ThemeService.TextMutDark);

            var stack = new VerticalStackLayout { Spacing = 0 };
            stack.Children.Add(header);
            stack.Children.Add(content);
            return SectionBorder(stack);
        }

        private static Border SectionBorder(View content)
        {
            var b = new Border
            {
                Padding = new Thickness(16),
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 16 },
                Shadow = new Shadow { Brush = Brush.Black, Opacity = 0.08f, Radius = 12, Offset = new Point(0, 3) },
                Content = content
            };
            b.SetAppThemeColor(Border.BackgroundColorProperty, ThemeService.CardLight, ThemeService.CardDark);
            return b;
        }

        private static Entry StyledEntry(string placeholder) => new()
        {
            Placeholder = placeholder,
            FontSize = 15,
            HeightRequest = 48,
            ClearButtonVisibility = ClearButtonVisibility.WhileEditing
        };
    }
}

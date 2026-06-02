using CityExplorer.Resources.Localization;
using Microsoft.Maui.Controls;

namespace CityExplorer.Views
{
    public partial class MainTabbedPage : TabbedPage
    {
        // ────────────────────────────────────────────────────────────────
        //  Конструктор с DI – все страницы передаются через параметры,
        //  чтобы не нарушать цепочку зависимостей MVVM.
        // ────────────────────────────────────────────────────────────────
        public MainTabbedPage(
            ExplorePage explorePage,
            FavoritesPage favoritesPage,
            SettingsPage settingsPage,
            AdminPage adminPage)
        {
            BuildUI(explorePage, favoritesPage, settingsPage, adminPage);
        }

        // Пустой конструктор – создаёт страницы без ViewModel (для превью)
        public MainTabbedPage()
        {
            BuildUI(new ExplorePage(), new FavoritesPage(),
                    new SettingsPage(), new AdminPage());
        }

        // ────────────────────────────────────────────────────────────────
        //  Построение TabbedPage программно
        // ────────────────────────────────────────────────────────────────
        private void BuildUI(
            ExplorePage explorePage,
            FavoritesPage favoritesPage,
            SettingsPage settingsPage,
            AdminPage adminPage)
        {
            Title = "CityExplorer";
            BarBackgroundColor = Color.FromArgb("#1A1A2E");
            BarTextColor = Colors.White;
            SelectedTabColor = Color.FromArgb("#E63946");
            UnselectedTabColor = Color.FromArgb("#888888");

            // ── Вкладка «Avasta» ────────────────────────────────────────
            explorePage.Title = AppResources.ExploreTab;
            explorePage.IconImageSource = "explore.png";

            // ── Вкладка «Lemmikud» ──────────────────────────────────────
            favoritesPage.Title = AppResources.FavoritesTab;
            favoritesPage.IconImageSource = "favorites.png";

            // ── Вкладка «Seaded» ────────────────────────────────────────
            settingsPage.Title = AppResources.SettingsTab;
            settingsPage.IconImageSource = "settings.png";

            // ── Вкладка «Admin» ─────────────────────────────────────────
            adminPage.Title = "⚙️ Admin";
            adminPage.IconImageSource = "admin.png";

            Children.Add(explorePage);
            Children.Add(favoritesPage);
            Children.Add(settingsPage);
            Children.Add(adminPage);
        }
    }
}
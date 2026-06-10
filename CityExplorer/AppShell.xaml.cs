using CityExplorer.Resources.Localization;

namespace CityExplorer;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }

    public void ReloadTabs()
    {
        if (Items.Count == 0) return;
        var tabBar = Items[0] as TabBar;
        if (tabBar == null) return;
        if (tabBar.Items.Count >= 4)
        {
            tabBar.Items[0].Title = AppResources.ExploreTab;
            tabBar.Items[1].Title = AppResources.FavoritesTab;
            tabBar.Items[2].Title = AppResources.SettingsTab;
            tabBar.Items[3].Title = AppResources.AdminTab;
        }
    }
}

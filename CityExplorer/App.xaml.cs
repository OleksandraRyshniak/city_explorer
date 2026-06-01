using CityExplorer.Views;

namespace CityExplorer;

/// <summary>
/// Rakenduse käivituspunkt.
/// MainTabbedPage on seatud peale leheküljeks (DI kaudu).
/// </summary>
public partial class App : Application
{
    public App(MainTabbedPage mainPage)
    {
        InitializeComponent();
        MainPage = mainPage;
    }
}

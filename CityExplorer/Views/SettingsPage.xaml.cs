using CityExplorer.ViewModels;
using Microsoft.Maui.Controls;

namespace CityExplorer.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public SettingsPage()
    {
        InitializeComponent();
    }
}

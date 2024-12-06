using ImageCollageCreator.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace ImageCollageCreator;

public partial class MainPage : ContentPage
{
    public ObservableCollection<Collage> Collages { get; set; } = new ObservableCollection<Collage>();

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private async void OnCreateCollageClicked(object sender, EventArgs e)
    {
        // Navigate to the Collage Page
        await Navigation.PushAsync(new CollagePage());
    }

    private void OnOpenCollageClicked(object sender, EventArgs e)
    {
        // Logic to open an existing collage
        // For now, this could be stubbed
    }
}

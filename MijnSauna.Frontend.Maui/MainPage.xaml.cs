using MijnSauna.Frontend.Maui.ViewModels;

namespace MijnSauna.Frontend.Maui;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
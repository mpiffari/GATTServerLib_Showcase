using GattServerLib.Interfaces;
using Microsoft.Extensions.Logging;

namespace Showcase;

public partial class MainPage : ContentPage
{
    public MainPage(IGattServer gattServer, ILogger logger, IPermissionHandler permissionHandler)
    {
        InitializeComponent();
        BindingContext = new MainPageViewModel(gattServer, logger, permissionHandler);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await ((IViewModelBase)BindingContext).OnAppearing();
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        await ((IViewModelBase)BindingContext).OnDisappearing();
    }
}
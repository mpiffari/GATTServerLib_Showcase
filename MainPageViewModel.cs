using System.Windows.Input;
using GattServerLib.Interfaces;
using Microsoft.Extensions.Logging;
using Showcase.Logger;

namespace Showcase;

public interface IViewModelBase
{
    Task OnAppearing();

    Task OnDisappearing();
}

public class MainPageViewModel : IViewModelBase
{
    private readonly IGattServer gattServer;
    private readonly ILogger logger;
    private readonly IPermissionHandler permissionHandler;
    
    public ICommand OnStartGattServerCommand { get; }
    public ICommand OnStopGattServerCommand { get; }
    
    public MainPageViewModel(IGattServer gattServer, ILogger logger, IPermissionHandler permissionHandler)
    {
        this.permissionHandler = permissionHandler;
        this.gattServer = gattServer;
        this.logger = logger;
        OnStartGattServerCommand = new Command(OnStartGattServer);
        OnStopGattServerCommand = new Command(OnStopGattServer);
    }
    public async Task OnAppearing()
    {
        await permissionHandler.CheckAndRequestPermissionsAsync();
        await gattServer.InitializeAsync(logger);
    }

    public Task OnDisappearing()
    {
        return Task.CompletedTask;
    }

    private async void OnStartGattServer()
    {
        var starAsyncResult = await gattServer.StartAdvertisingAsync();
        logger.LogDebug(LoggerScope.GATT_SERVER_LIB_CONSUMER.EventId(), "Start advertising result: {R}", starAsyncResult);
    }
    
    private async void OnStopGattServer()
    {
        await gattServer.StopAdvertisingAsync();
        logger.LogDebug(LoggerScope.GATT_SERVER_LIB_CONSUMER.EventId(), "Stop advertising");
    }
}
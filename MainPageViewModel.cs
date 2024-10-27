using System.Windows.Input;
using GattServerLib.GattOptions;
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
        // Complete UUID (standard UUUID + base UUID)
        var disUuid = new Guid("0000180A-0000-1000-8000-00805F9B34FB");
        IBleService disService = new BleService("Device info", disUuid);
        await disService.AddCharacteristicAsync(new BleCharacteristic("Information", BleCharacteristicProperties.Read));
        
        var batteryUuid = new Guid("0000180F-0000-1000-8000-00805F9B34FB");
        IBleService batteryService = new BleService("Battery info", batteryUuid);
        await disService.AddCharacteristicAsync(new BleCharacteristic("Battery level", BleCharacteristicProperties.Read));
        
        var customUuid = new Guid("12345678-0000-1000-1996-00805F9B34FB");
        IBleService customService = new BleService("Custom service", customUuid);
        await disService.AddCharacteristicAsync(new BleCharacteristic("Custom", BleCharacteristicProperties.Read));
        
        var advOptions = new BleAdvOptions
        {
            LocalName = "MICHELE_DEVICE",
            ServiceUuids = new string[] { customUuid.ToString() }
        };
        
        var starAsyncResult = await gattServer.StartAdvertisingAsync(advOptions);
        logger.LogDebug(LoggerScope.GATT_SERVER_LIB_CONSUMER.EventId(), "Start advertising result: {R}", starAsyncResult);
        
        var addServiceResult = await gattServer.AddServiceAsync(disService);
        logger.LogDebug(LoggerScope.GATT_SERVER_LIB_CONSUMER.EventId(), "Add service {S} result: {R}", disUuid.ToString(), addServiceResult);
        
        addServiceResult = await gattServer.AddServiceAsync(batteryService); 
        logger.LogDebug(LoggerScope.GATT_SERVER_LIB_CONSUMER.EventId(), "Add service {S} result: {R}", disUuid.ToString(), addServiceResult);

        addServiceResult = await gattServer.AddServiceAsync(customService);
        logger.LogDebug(LoggerScope.GATT_SERVER_LIB_CONSUMER.EventId(), "Add service {S} result: {R}", disUuid.ToString(), addServiceResult);
    }
    
    private async void OnStopGattServer()
    {
        await gattServer.StopAdvertisingAsync();
        logger.LogDebug(LoggerScope.GATT_SERVER_LIB_CONSUMER.EventId(), "Stop advertising");
    }
}
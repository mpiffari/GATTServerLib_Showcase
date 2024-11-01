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
    
    public ICommand OnInitGattServerCommand { get; }
    public ICommand OnStartGattServerCommand { get; }
    public ICommand OnStopGattServerCommand { get; }
    
    private static Guid customUuid = Guid.Parse("12345678-0000-1000-1996-00805F9B34FB");
    private static Guid customUuidCharact = Guid.Parse("CC3456CC-0000-1000-1996-00805F9B34FB");
    private static Guid sDisUuid = Guid.Parse("0000180A-0000-1000-8000-00805F9B34FB");
    private static Guid cDisUuid = Guid.Parse("C000180A-0000-1000-8000-00805F9B34FB");
    private static Guid sBatteryUuid = Guid.Parse("0000180F-0000-1000-8000-00805F9B34FB");
    private static Guid cBatteryUuid = Guid.Parse("C000180F-0000-1000-8000-00805F9B34FB");
    
    public MainPageViewModel(IGattServer gattServer, ILogger logger, IPermissionHandler permissionHandler)
    {
        this.permissionHandler = permissionHandler;
        this.gattServer = gattServer;
        this.logger = logger;
        OnInitGattServerCommand = new Command(OnInitGattServer);
        OnStartGattServerCommand = new Command(OnStartGattServer);
        OnStopGattServerCommand = new Command(OnStopGattServer);
        
        gattServer.OnRead = OnRead;
    }

    private (bool, byte[]) OnRead((string cUuid, int offset) arg)
    {
        var c = Guid.Parse(arg.cUuid);
        if (c == cDisUuid)
        {
            logger.LogDebug(LoggerScope.GATT_SERVER_LIB_CONSUMER.EventId(), "Preparing response for device information...");

            var bytes = "S21_OF_MINE"u8.ToArray();
            return (true, bytes);
        }
        
        if (c == cBatteryUuid)
        {
            logger.LogDebug(LoggerScope.GATT_SERVER_LIB_CONSUMER.EventId(), "Preparing response for battery...");
            return (true, new byte[] { 0x0A }); // 10%
        }
        
        logger.LogDebug(LoggerScope.GATT_SERVER_LIB_CONSUMER.EventId(), "Preparing response for other characteristic...");
        return (false, new byte[] { 0xFF });
    }

    public async Task OnAppearing()
    {
        var permissionAsyncResult = await permissionHandler.CheckAndRequestPermissionsAsync();
        logger.LogDebug(LoggerScope.GATT_SERVER_LIB_CONSUMER.EventId(), "Permission check result: {R}", permissionAsyncResult);

        var isBleStatusEnable = await permissionHandler.IsBluetoothEnabledAsync();
        if (!isBleStatusEnable)
        {
            await permissionHandler.RequestBluetoothActivationAsync();
        }
    }

    public Task OnDisappearing()
    {
        return Task.CompletedTask;
    }

    private async void OnInitGattServer()
    {
        await gattServer.InitializeAsync(logger);
    }
    
    private async void OnStartGattServer()
    {
        // Complete UUID (standard UUUID + base UUID)
        var devInfoCharacts = new List<IBleCharacteristic>
        {
            new BleCharacteristic("Information", cDisUuid, BleCharacteristicProperties.Read)
        };
        IBleService disService = new BleService("Device info", sDisUuid, devInfoCharacts);

        var batteryCharacts = new List<IBleCharacteristic>
        {
            new BleCharacteristic("Battery level", cBatteryUuid, BleCharacteristicProperties.Read)
        };
        IBleService batteryService = new BleService("Battery info", sBatteryUuid, batteryCharacts);
    
        var customCharacts = new List<IBleCharacteristic>
        {
            new BleCharacteristic("Custom", customUuidCharact, BleCharacteristicProperties.Read)
        };
        IBleService customService = new BleService("Custom service", customUuid, customCharacts);
    
        var advOptions = new BleAdvOptions
        {
            LocalName = "MICHELE_DEVICE",
            ServiceUuids = new string[] { sDisUuid.ToString() }
        };
    
        var starAsyncResult = await gattServer.StartAdvertisingAsync(advOptions);
        logger.LogDebug(LoggerScope.GATT_SERVER_LIB_CONSUMER.EventId(), "Start advertising result: {R}", starAsyncResult);
    
        var addServiceResult = await gattServer.AddServiceAsync(disService);
        logger.LogDebug(LoggerScope.GATT_SERVER_LIB_CONSUMER.EventId(), "Add service {S} result: {R}", sDisUuid.ToString(), addServiceResult);
    
        addServiceResult = await gattServer.AddServiceAsync(batteryService); 
        logger.LogDebug(LoggerScope.GATT_SERVER_LIB_CONSUMER.EventId(), "Add service {S} result: {R}", sDisUuid.ToString(), addServiceResult);

        addServiceResult = await gattServer.AddServiceAsync(customService);
        logger.LogDebug(LoggerScope.GATT_SERVER_LIB_CONSUMER.EventId(), "Add service {S} result: {R}", sDisUuid.ToString(), addServiceResult);
    }
    
    private async void OnStopGattServer()
    {
        await gattServer.StopAdvertisingAsync();
        logger.LogDebug(LoggerScope.GATT_SERVER_LIB_CONSUMER.EventId(), "Stop advertising");
    }
}
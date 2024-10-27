using GattServerLib;
using GattServerLib.Interfaces;
using Microsoft.Extensions.Logging;

namespace Showcase;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        
#if ANDROID
        builder.Services.AddTransient<IGattServer, AndroidGattServer>();
        builder.Services.AddTransient<IPermissionHandler, AndroidPermissionHandler>();
#elif IOS
        builder.Services.AddTransient<IGattServer, iOSGattServer>();
        builder.Services.AddTransient<IPermissionHandler, iOSPermissionHandler>();
#endif
        builder.Services.AddTransient<ILogger, Logger.Logger>();
        builder.Services.AddSingleton<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
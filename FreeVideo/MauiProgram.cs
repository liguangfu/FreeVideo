using CommunityToolkit.Maui;
using FreeVideo.Data;
using FreeVideo.Pages;
using FreeVideo.ViewModels;
using Microsoft.Extensions.Logging;
using FreeVideo.Services;

namespace FreeVideo;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMediaElement()
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
            

        builder.Services.AddSingleton<VideoDatabase>();
        builder.Services.AddSingleton<ISearchVideoService, zyk1080SearchVideoService>();


        builder.Services.AddTransient<MainViewModel>().AddTransient<MainPage>();
        //Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));

        builder.Services.AddTransient<ShowVideoModel>().AddTransient<ShowVideoPage>();
        //Routing.RegisterRoute(nameof(ShowVideoPage), typeof(ShowVideoPage));

        builder.Services.AddTransient<PlayVideoViewModel>().AddTransient<PlayVideoPage>();
        //Routing.RegisterRoute(nameof(PlayVideoPage), typeof(PlayVideoPage));

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

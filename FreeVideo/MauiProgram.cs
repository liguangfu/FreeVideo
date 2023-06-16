using CommunityToolkit.Maui;
using FreeVideo.Data;
using FreeVideo.Pages;
using Microsoft.Extensions.Logging;
using Video.Handlers;

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
			})
			.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler(typeof(Video.Controls.Video), typeof(VideoHandler));
            });

		//builder.Services.AddTransientWithShellRoute<ShowVideoPage, ShowVideoModel>(AppShell.GetPageRoute<ShowVideoModel>());

        builder.Services.AddSingleton<VideoDatabase>();

        builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

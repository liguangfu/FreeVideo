using FreeVideo.Pages;
using FreeVideo.ViewModels;

namespace FreeVideo;

public partial class AppShell : Shell
{
    public Dictionary<string, Type> Routes { get; private set; } = new Dictionary<string, Type>();

    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();
    }
    void RegisterRoutes()
    {
        Routes.Add("showVideoPage", typeof(ShowVideoPage));
        Routes.Add("playVideoPage", typeof(PlayVideoPage));
        Routes.Add("downVideoPage", typeof(DownVideoPage));

        foreach (var item in Routes)
        {
            Routing.RegisterRoute(item.Key, item.Value);
        }
    }
}

using FreeVideo.Pages;

namespace FreeVideo;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();
    }
    void RegisterRoutes()
    {
        Routing.RegisterRoute("showVideoPage", typeof(ShowVideoPage));
        Routing.RegisterRoute("playVideoPage", typeof(PlayVideoPage));
    }
}

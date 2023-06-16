namespace FreeVideo.Pages;

public class ViewsGalleryPage : BaseGalleryPage<ViewModels.ViewsGalleryViewModel>
{
    public ViewsGalleryPage(IDeviceInfo deviceInfo, ViewModels.ViewsGalleryViewModel viewsGalleryViewModel)
        : base("Views", deviceInfo, viewsGalleryViewModel)
    {
    }
}

using FreeVideo.ViewModels;

namespace FreeVideo.Pages;

public partial class ShowVideoPage : BasePage<ShowVideoModel>
{
    public ShowVideoPage(ShowVideoModel vm) : base(vm)
    {
        InitializeComponent();
    }
}
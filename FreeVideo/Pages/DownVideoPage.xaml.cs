using FreeVideo.ViewModels;

namespace FreeVideo.Pages;

public partial class DownVideoPage : BasePage<DownVideoViewModel>
{
	public DownVideoPage(DownVideoViewModel vm):base(vm)
	{
		InitializeComponent();
	}
}
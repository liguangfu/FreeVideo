using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeVideo.Models;

namespace FreeVideo.ViewModels;

public sealed class ViewsGalleryViewModel : BaseGalleryViewModel
{
    public ViewsGalleryViewModel()
        : base(new[]
        {

            SectionModel.Create<MainViewModel>("MainViewModel", Colors.Red, "MediaElement is a view for playing video and audio"),
            SectionModel.Create<ShowVideoModel>("ShowVideoModel", Colors.Red, "A page demonstrating multiple different Popups"),
            
        })
    {
    }
}

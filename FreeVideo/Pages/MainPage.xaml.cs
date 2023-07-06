using FreeVideo.Data;

namespace FreeVideo.Pages;

public partial class MainPage : BasePage<ViewModels.MainViewModel>
{
    private readonly VideoDatabase _videoDatabase;
    public MainPage(ViewModels.MainViewModel vm, VideoDatabase videoDatabase) : base(vm)
    {
        InitializeComponent();
        _videoDatabase = videoDatabase;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var hisVideoList = await _videoDatabase.GetHisVideoListAsync();
        if (hisVideoList != null)
        {
            cvHisVideo.ItemsSource = hisVideoList;
        }
    }

}


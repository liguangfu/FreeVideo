using FreeVideo.Data;

namespace FreeVideo.Pages;

public partial class MainPage : ContentPage
{
    private readonly VideoDatabase _videoDatabase;
    public MainPage(VideoDatabase videoDatabase)
    {
        InitializeComponent();
        _videoDatabase = videoDatabase;
        // BindingContext


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

    private void ContentPage_NavigatedTo(object sender, NavigatedToEventArgs e)
    {
        if (searchResult.SelectedItem != null)
        {
            searchResult.SelectedItem = null;
        }
        
        if (cvHisVideo.SelectedItem != null)
        {
            cvHisVideo.SelectedItem = null;
        }
    }
}


using Microsoft.Maui.Controls.PlatformConfiguration;
using FreeVideo.ViewModels;
using System.Linq;
using CommunityToolkit.Maui.Views;
using System.Diagnostics;
#if ANDROID

using Android.Support.V4.Media.Session;
using Android.Widget;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Audio;
using Com.Google.Android.Exoplayer2.Metadata;
using Com.Google.Android.Exoplayer2.Text;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.UI;
using Com.Google.Android.Exoplayer2.Video;

#endif

namespace FreeVideo.Pages;

public partial class PlayVideoPage : BasePage<PlayVideoViewModel>
{
    private Data.VideoDatabase videoDatabase;

    public PlayVideoPage(PlayVideoViewModel vm, Data.VideoDatabase videoDatabase) : base(vm)
    {
        InitializeComponent();
        this.videoDatabase = videoDatabase;
    }

    private double width = 0;
    private double height = 0;

    /// <summary>
    /// 监听变化
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (this.width != width || this.height != height)
        {
            this.width = width;
            this.height = height;
            Debug.WriteLine($"old screen width: {this.width}");
            Debug.WriteLine($"old screen height: {this.height}");
            Debug.WriteLine($"old screen width: {width}");
            Debug.WriteLine($"old screen height: {height}");

            Debug.WriteLine($"old mediaElement width: {mediaElement.Width}");
            Debug.WriteLine($"old mediaElement height: {mediaElement.Height}");

            // Get Metrics
            var mainDisplayInfo = DeviceDisplay.Current.MainDisplayInfo;


            //Portrait 竖屏  Landscape 横屏
            var orientation = mainDisplayInfo.Orientation;

            if (orientation == DisplayOrientation.Portrait)
            {
                mediaElement.WidthRequest = width;
                mediaElement.HeightRequest = 700;
                mediaElement.Aspect = Aspect.Center;
                Shell.SetNavBarIsVisible(this, true);

            }
            else if (orientation == DisplayOrientation.Landscape)
            {
                mediaElement.WidthRequest = width;
                mediaElement.HeightRequest = height;
                mediaElement.Aspect = Aspect.AspectFit;

                Shell.SetNavBarIsVisible(this, false);

#if ANDROID

                var activity = Platform.CurrentActivity;

                if (activity?.Window is null)
                {
                    return;
                }
                //SetBarStatus(fullScreenStatus);

                AndroidX.Core.View.WindowCompat.SetDecorFitsSystemWindows(activity.Window, false);

                var windowInsetsControllerCompat = AndroidX.Core.View.WindowCompat.GetInsetsController(activity.Window, activity.Window.DecorView);
                var types = AndroidX.Core.View.WindowInsetsCompat.Type.StatusBars()
                            | AndroidX.Core.View.WindowInsetsCompat.Type.NavigationBars();


                windowInsetsControllerCompat.SystemBarsBehavior = AndroidX.Core.View.WindowInsetsControllerCompat.BehaviorShowBarsBySwipe;
                windowInsetsControllerCompat.Hide(types);
                return;

                windowInsetsControllerCompat.Show(types);

#endif

            }
            

            Debug.WriteLine($"mediaElement width: {mediaElement.Width}");
            Debug.WriteLine($"mediaElement height: {mediaElement.Height}");

        }
    }

    //protected override bool OnBackButtonPressed()
    //{
    //    //base.OnBackButtonPressed();
    //    //await Shell.Current.GoToAsync($"ShowVideoPage?vod_id={BindingContext.VodId}&vod_play_from=");
    //    return true;
    //}

    private async void ContentPage_Unloaded(object sender, EventArgs e)
    {
        //记录当前播放
        BindingContext.HisVideo.current_duration = mediaElement.Position;
        BindingContext.HisVideo.current_play = BindingContext.CurrentPlay;

        await this.videoDatabase.SaveHisVideoAsync(BindingContext.HisVideo);

        mediaElement.Handler?.DisconnectHandler();
    }

    private bool fullScreenStatus { get; set; } = false;
    private void onClickFullScreen(object sender, EventArgs e)
    {
        fullScreenStatus = !fullScreenStatus;

#if ANDROID

        var activity = Platform.CurrentActivity;

        if (activity?.Window is null)
        {
            return;
        }
        //SetBarStatus(fullScreenStatus);

        AndroidX.Core.View.WindowCompat.SetDecorFitsSystemWindows(activity.Window, false);

        var windowInsetsControllerCompat = AndroidX.Core.View.WindowCompat.GetInsetsController(activity.Window, activity.Window.DecorView);
        var types = AndroidX.Core.View.WindowInsetsCompat.Type.StatusBars()
                    | AndroidX.Core.View.WindowInsetsCompat.Type.NavigationBars();

        if (fullScreenStatus)
        {
            windowInsetsControllerCompat.SystemBarsBehavior = AndroidX.Core.View.WindowInsetsControllerCompat.BehaviorShowBarsBySwipe;
            windowInsetsControllerCompat.Hide(types);
            return;
        }

        windowInsetsControllerCompat.Show(types);

#endif

    }

    private void positionChanged(object sender, CommunityToolkit.Maui.Core.Primitives.MediaPositionChangedEventArgs e)
    {

    }

    private void mediaEnded(object sender, EventArgs e)
    {
        if (BindingContext.VodPlayUrl != null && BindingContext.VodPlayUrl.Count > 0)
        {
            bool next = false;
            foreach (var item in BindingContext.VodPlayUrl)
            {
                if (next)
                {
                    BindingContext.CurrentPosition = TimeSpan.Zero;
                    BindingContext.CurrentPlay = item.name;
                    BindingContext.CurrentPlayUrl = item.url;
                    break;
                }
                if (item.name == BindingContext.CurrentPlay)
                {
                    next = true;
                }
            }
        }
    }

    private void mediaOpened(object sender, EventArgs e)
    {
        if (BindingContext.CurrentPosition != TimeSpan.Zero)
        {
            mediaElement.SeekTo(BindingContext.CurrentPosition);
        }
    }
}
using Microsoft.Maui.Controls.PlatformConfiguration;
#if ANDROID

﻿using Android.Support.V4.Media.Session;
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

public partial class PlayVideoPage : ContentPage
{
    public PlayVideoPage()
    {
        InitializeComponent();
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

        //if (this.width != width || this.height != height)
        //{
        this.width = width;
        this.height = height;

        Console.WriteLine("screen width " + width);
        Console.WriteLine("screen height " + height);


        // Get Metrics
        var mainDisplayInfo = DeviceDisplay.Current.MainDisplayInfo;


        //Portrait 竖屏  Landscape 横屏
        var orientation = mainDisplayInfo.Orientation;

        if (orientation == DisplayOrientation.Portrait)
        {
            mediaElement.WidthRequest = width;
            mediaElement.HeightRequest = 500;
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
        //}
    }

    private void ContentPage_Unloaded(object sender, EventArgs e)
    {
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
}
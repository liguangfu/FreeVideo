#if IOS || MACCATALYST
using PlatformView = Video.Platforms.MaciOS.MauiVideoPlayer;
#elif ANDROID
using PlatformView = Video.Platforms.Android.MauiVideoPlayer;
#elif WINDOWS
using PlatformView = Video.Platforms.Windows.MauiVideoPlayer;
#elif (NETSTANDARD || !PLATFORM) || (NET7_0_OR_GREATER && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif
using Video.Controls;
using Microsoft.Maui.Handlers;

namespace Video.Handlers
{
    public partial class VideoHandler
    {
        public static IPropertyMapper<Video.Controls.Video, VideoHandler> PropertyMapper = new PropertyMapper<Video.Controls.Video, VideoHandler>(ViewHandler.ViewMapper)
        {
            [nameof(Video.Controls.Video.AreTransportControlsEnabled)] = MapAreTransportControlsEnabled,
            [nameof(Video.Controls.Video.Source)] = MapSource,
            [nameof(Video.Controls.Video.IsLooping)] = MapIsLooping,
            [nameof(Video.Controls.Video.Position)] = MapPosition
        };

        public static CommandMapper<Video.Controls.Video, VideoHandler> CommandMapper = new(ViewCommandMapper)
        {
            [nameof(Video.Controls.Video.UpdateStatus)] = MapUpdateStatus,
            [nameof(Video.Controls.Video.PlayRequested)] = MapPlayRequested,
            [nameof(Video.Controls.Video.PauseRequested)] = MapPauseRequested,
            [nameof(Video.Controls.Video.StopRequested)] = MapStopRequested
        };

        public VideoHandler() : base(PropertyMapper, CommandMapper)
        {
        }
    }

}

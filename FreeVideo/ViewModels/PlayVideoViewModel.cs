using CommunityToolkit.Maui.Core.Primitives;
using FreeVideo.Data;
using FreeVideo.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FreeVideo.ViewModels
{
    public class PlayVideoViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly VideoDatabase videoDatabase;

        public ICommand BackShowVideoCommand { get; }

        public PlayVideoViewModel(VideoDatabase videoDatabase)
        {
            this.videoDatabase = videoDatabase;

            BackShowVideoCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync($"..?vod_id={VodId}&vod_play_from=");
            });
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("vod_id") && query["vod_id"] != null)
            {
                VodId = query["vod_id"].ToString();

                if (query.ContainsKey("vodPlayUrls") && query["vodPlayUrls"] != null)
                {
                    VodPlayUrl = query["vodPlayUrls"] as ObservableCollection<VideoPlayListModel>;
                }

                //获取历史记录
                HisVideo = await this.videoDatabase.GetHisVideoAsync(VodId);

                //获取播放方式
                if (query.ContainsKey("playMode") && query["playMode"] != null)
                {
                    var playMode = query["playMode"].ToString();
                    if (playMode == "go_play" && !string.IsNullOrEmpty(HisVideo.current_play))//继续播放
                    {
                        CurrentPlay = HisVideo.current_play;
                        CurrentPosition = HisVideo.current_duration;
                        CurrentPlayUrl = VodPlayUrl.FirstOrDefault(it => it.name.Equals(HisVideo.current_play))?.getPlayUrl();
                    }
                    else if (query.ContainsKey("select_play_url") && query["select_play_url"] != null)
                    {
                        CurrentPosition = TimeSpan.Zero;
                        CurrentPlay = query["select_play"]?.ToString();
                        CurrentPlayUrl = VodPlayUrl.FirstOrDefault(it => it.name.Equals(CurrentPlay))?.getPlayUrl();
                    }
                    else
                    {
                        CurrentPosition = TimeSpan.Zero;
                        CurrentPlayUrl = VodPlayUrl.FirstOrDefault()?.getPlayUrl();
                        CurrentPlay = VodPlayUrl.FirstOrDefault()?.name;
                    }
                }

                if (HisVideo != null)
                {
                    VodName = HisVideo.vod_name;
                }
            }
        }


        private ObservableCollection<VideoPlayListModel> vod_play_url { get; set; } = new ObservableCollection<VideoPlayListModel>();
        /// <summary>
        /// 当前播放列表
        /// </summary>
        public ObservableCollection<VideoPlayListModel> VodPlayUrl
        {
            get => vod_play_url;
            set
            {
                vod_play_url = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 当前播放路径
        /// </summary>
        private string current_play_url { get; set; }
        /// <summary>
        /// 当前播放路径
        /// </summary>
        public string CurrentPlayUrl
        {
            get => current_play_url;
            set
            {
                current_play_url = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// 当前播放，如：HD国语版
        /// </summary>
        private string current_play { get; set; }
        /// <summary>
        /// 当前播放，如：HD国语版
        /// </summary>
        public string CurrentPlay
        {
            get => current_play;
            set
            {
                current_play = value;
                OnPropertyChanged();
            }
        }


        private TimeSpan current_position { get; set; }
        public TimeSpan CurrentPosition
        {
            get { return current_position; }
            set
            {
                current_position = value;
            }
        }



        private string vod_id { get; set; }
        public string VodId
        {
            get { return vod_id; }
            set
            {
                vod_id = value;
                OnPropertyChanged();
            }
        }
        private string vod_name { get; set; }
        public string VodName
        {
            get { return vod_name; }
            set
            {
                vod_name = value;
                OnPropertyChanged();
            }
        }

        private VideoHistoryModel his_video { get; set; }
        public VideoHistoryModel HisVideo
        {
            get { return his_video; }
            set
            {
                his_video = value;
                OnPropertyChanged();
            }
        }

    }
}

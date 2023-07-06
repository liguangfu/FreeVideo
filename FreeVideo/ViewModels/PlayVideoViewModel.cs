using FreeVideo.Models;
using FreeVideo.Services;
using System.Collections.ObjectModel;

namespace FreeVideo.ViewModels
{
    public class PlayVideoViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IHistoryVideoService _historyVideoService;


        public PlayVideoViewModel(IHistoryVideoService historyVideoService)
        {
            _historyVideoService = historyVideoService;
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("vod_id") && query["vod_id"] != null)
            {
                VodId = query["vod_id"].ToString();
                HisVideo = await _historyVideoService.GetHisVideoAsync(VodId);
            }

            if (query.ContainsKey("VodPlayUrl") && query["VodPlayUrl"] != null)
            {
                VodPlayUrl = query["VodPlayUrl"] as ObservableCollection<VideoPlayListModel>;
            }

            if (query.ContainsKey("select_play") && query["select_play"] != null)
            {
                CurrentPlay = query["select_play"].ToString();
            }
            if (query.ContainsKey("select_play_url") && query["select_play_url"] != null)
            {
                CurrentPlayUrl = query["select_play_url"].ToString();
            }
            else if (HisVideo != null && !string.IsNullOrEmpty(HisVideo.current_play))
            {
                CurrentPlay = HisVideo.current_play;

                CurrentPlayUrl = VodPlayUrl.Where(it => it.name.Equals(HisVideo.current_play)).FirstOrDefault()?.url;
            }
            else
            {
                CurrentPlayUrl = VodPlayUrl.FirstOrDefault()?.url;
                CurrentPlay = VodPlayUrl.FirstOrDefault()?.name;
            }

            if (HisVideo != null)
            {
                VodName = HisVideo.vod_name;
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

        /// <summary>
        /// 当前播放位置
        /// </summary>
        private TimeSpan current_duration { get; set; }
        /// <summary>
        /// 当前播放位置
        /// </summary>
        public TimeSpan CurrentDuration
        {
            get => current_duration;
            set
            {
                current_duration = value;
                OnPropertyChanged();
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

        private TimeSpan current_position { get; set; }
        public TimeSpan CurrentPosition
        {
            get { return current_position; }
            set
            {
                current_position = value;
            }
        }

    }
}

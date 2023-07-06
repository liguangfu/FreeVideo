using FreeVideo.Data;
using FreeVideo.Models;
using FreeVideo.Pages;
using FreeVideo.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FreeVideo.ViewModels;

public class ShowVideoModel : BaseViewModel, IQueryAttributable
{
    private readonly ISearchVideoService searchVideoService;
    private readonly VideoDatabase videoDatabase;

    public ICommand PlayVideoCommand { get; }
    public ICommand GoPlayCommand { get; }
    public ICommand BackMainCommand { get; }

    public ShowVideoModel(ISearchVideoService searchVideoService, VideoDatabase videoDatabase)
    {
        this.searchVideoService = searchVideoService;
        this.videoDatabase = videoDatabase;

        PlayVideoCommand = new Command<VideoPlayListModel>(async (VideoPlayListModel vod) =>
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "vod_id", VodId },
                { "playMode", "select_play" },
                { "select_play", vod.name },
                { "select_play_url", vod.url },
                { "vodPlayUrls", VodPlayUrl },
            };
            await Shell.Current.GoToAsync($"playVideoPage", navigationParameter);
        });

        GoPlayCommand = new Command(async () =>
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "vod_id", VodId },
                { "playMode", "go_play" },
                { "vodPlayUrls", VodPlayUrl },
            };
            await Shell.Current.GoToAsync($"playVideoPage", navigationParameter);
        });


        BackMainCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync($"..");
        });
    }


    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("vod_id"))
        {
            string id = query["vod_id"].ToString();
            if (query.ContainsKey("vod_play_from"))
            {
                string vod_play_from = query["vod_play_from"].ToString();
            }

            var searchResult = await searchVideoService.GetSearchDetailAsync(id);
            if (searchResult != null)
            {
                VodId = searchResult.vod_id;
                VodName = searchResult.vod_name;
                VodPic = searchResult.vod_pic;
                VodRemarks = searchResult.vod_remarks;
                VodContent = searchResult.vod_content;
                VodPlayUrl = new ObservableCollection<VideoPlayListModel>(searchResult.vod_play_list);
            }

            //查询历史记录
            var hisVideo = await this.videoDatabase.GetHisVideoAsync(id);
            if (hisVideo != null)
            {
                CurrentDuration = hisVideo.current_duration;
                CurrentPlay = hisVideo.current_play;
                hisVideo.show_time = DateTime.Now;
                hisVideo.vod_remarks = searchResult.vod_remarks;
                hisVideo.vod_play_url = searchResult.vod_play_url;
            }
            else
            {
                hisVideo = new VideoHistoryModel()
                {
                    vod_id = searchResult.vod_id,
                    vod_actor = searchResult.vod_actor,
                    vod_play_url = searchResult.vod_play_url,
                    vod_content = searchResult.vod_content,
                    vod_en = searchResult.vod_en,
                    vod_name = searchResult.vod_name,
                    vod_pic = searchResult.vod_pic,
                    vod_play_from = searchResult.vod_play_from,
                    vod_remarks = searchResult.vod_remarks,
                    show_time = DateTime.Now
                };
            }
            await this.videoDatabase.SaveHisVideoAsync(hisVideo);
        }
    }

    #region 属性

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

    private string vod_pic { get; set; }
    public string VodPic
    {
        get { return vod_pic; }
        set
        {
            vod_pic = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 视频备注
    /// </summary>
    private string vod_remarks { get; set; } //"全36集"
    public string VodRemarks
    {
        get { return vod_remarks; }
        set
        {
            vod_remarks = value;
            OnPropertyChanged();
        }
    }

    private string vod_content { get; set; }
    public string VodContent
    {
        get { return vod_content; }
        set
        {
            vod_content = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<VideoPlayListModel> vod_play_url { get; set; } = new ObservableCollection<VideoPlayListModel>();
    public ObservableCollection<VideoPlayListModel> VodPlayUrl
    {
        get { return vod_play_url; }
        set
        {
            vod_play_url = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 历史播放，如：HD国语版
    /// </summary>
    private string current_play { get; set; }
    public string CurrentPlay
    {
        get { return current_play; }
        set
        {
            current_play = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 历史播放位置
    /// </summary>
    private TimeSpan current_duration { get; set; }
    public TimeSpan CurrentDuration
    {
        get { return current_duration; }
        set
        {
            current_duration = value;
            OnPropertyChanged();
        }
    }

    #endregion


}

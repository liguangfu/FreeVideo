using CommunityToolkit.Mvvm.Input;
using FreeVideo.Models;
using FreeVideo.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using static Android.Graphics.ColorSpace;

namespace FreeVideo.ViewModels;

public class ShowVideoModel : BaseViewModel, IQueryAttributable
{
    private readonly IHistoryVideoService _historyVideoService;

    public ShowVideoModel(IHistoryVideoService historyVideoService)
    {
        _historyVideoService = historyVideoService;

        _lazyPlayVideoCommand = new Lazy<AsyncRelayCommand<VideoPlayListModel>>(new AsyncRelayCommand<VideoPlayListModel>(PlayVideoCommandFunction));
    }


    public AsyncRelayCommand<VideoPlayListModel> PlayVideoCommand => _lazyPlayVideoCommand.Value;
    private Lazy<AsyncRelayCommand<VideoPlayListModel>> _lazyPlayVideoCommand;
    public async Task PlayVideoCommandFunction(VideoPlayListModel vod)
    {
        var navigationParameter = new Dictionary<string, object>
            {
                { "vod_id", VodId },
                { "select_play", vod.name },
                { "select_play_url", vod.url },
                { "VodPlayUrl", VodPlayUrl },
            };
        await Shell.Current.GoToAsync($"playVideoPage", navigationParameter);
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
            HisVideo = await _historyVideoService.GetHisVideoAsync(id);

            VodId = HisVideo.vod_id;
            VodName = HisVideo.vod_name;
            VodPic = HisVideo.vod_pic;
            VodRemarks = HisVideo.vod_remarks;
            VodContent = HisVideo.vod_content;
            var list = HisVideo.vod_play_url.Split('#');
            foreach (var item in list)
            {
                var play_url = item.Split('$');
                if (play_url.Length == 2)
                {
                    VodPlayUrl.Add(new VideoPlayListModel() { name = play_url[0], url = play_url[1] });
                }
            }
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
    /// 
    /// </summary>
    private VideoHistoryModel hisVideo { get; set; }
    public VideoHistoryModel HisVideo
    {
        get { return hisVideo; }
        set
        {
            hisVideo = value;
            OnPropertyChanged();
        }
    }

    #endregion


}

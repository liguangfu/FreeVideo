using CommunityToolkit.Mvvm.Input;
using FreeVideo.Data;
using FreeVideo.Models;
using N_m3u8DL_RE.MAUI;
using System.Collections.ObjectModel;


namespace FreeVideo.ViewModels;

public partial class DownVideoViewModel : BaseViewModel, IQueryAttributable
{
    private readonly VideoDatabase videoDatabase;

    //public ICommand DownVideoCommand { get; }
    public IAsyncRelayCommand<VideoPlayListModel> DownVideoCommand { get; }

    public DownVideoViewModel(VideoDatabase videoDatabase)
    {
        this.videoDatabase = videoDatabase;

        DownVideoCommand = new AsyncRelayCommand<VideoPlayListModel>(DownVideoAsync);

    }

    public async Task DownVideoAsync(VideoPlayListModel vod)
    {
        try
        {

            string savePath = Path.Combine(FileSystem.AppDataDirectory, VodId);
            string tmpPath = savePath;
            var task = new M3U8DownloadTask();

            var result = await task.DoWorkAsync(new M3U8DownloadArgs()
            {
                SaveName = vod.name,
                SavePath = savePath,
                TmpDir = tmpPath,
                TaskCount = 5,
                IsDelSubTs = true,//删除TS片段
                Url = vod.url
            });
            if (result)
            {
                vod.local_path =Path.Combine(savePath, vod.name+".mp4");
                await this.videoDatabase.SaveVideoPlayUrlAsync(vod);
            }
        }
        catch (Exception ex)
        {

        }
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {

        VodId = query["vod_id"].ToString();
        VodName = query["vod_name"].ToString();

        VodPlayList = query["vodPlayUrls"] as ObservableCollection<VideoPlayListModel>;
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

    private ObservableCollection<VideoPlayListModel> vod_play_list { get; set; } = new ObservableCollection<VideoPlayListModel>();
    public ObservableCollection<VideoPlayListModel> VodPlayList
    {
        get { return vod_play_list; }
        set
        {
            vod_play_list = value;
            OnPropertyChanged();
        }
    }

    #endregion


}

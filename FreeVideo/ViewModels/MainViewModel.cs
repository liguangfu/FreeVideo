using FreeVideo.Data;
using FreeVideo.Models;
using FreeVideo.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FreeVideo.ViewModels;

public class MainViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ISearchVideoService searchVideoService;
    private readonly VideoDatabase videoDatabase;

    public ICommand PerformSearch { get; }

    public ICommand SelectSearchResultCommand { get; }

    public ICommand SelectHisVideoCommand { get; }

    public ICommand DeleteHisVideoCommand { get; }
    public MainViewModel()
    {
        this.searchVideoService = new zyk1080SearchVideoService();
        this.videoDatabase = new VideoDatabase();


        PerformSearch = new Command<string>(async (string query) =>
        {
            SearchResults.Clear();
            SearchResults = new ObservableCollection<SearchVideoListModel>(await searchVideoService.GetSearchListAsync(query));
            GetHisVideoListAsync();
        });

        SelectSearchResultCommand = new Command<SearchVideoListModel>(async (SearchVideoListModel selectItem) =>
        {
            await Shell.Current.GoToAsync($"showVideoPage?vod_id={selectItem.vod_id}&vod_play_from={selectItem.vod_play_from}");
        });

        SelectHisVideoCommand = new Command<VideoHistoryModel>(async (VideoHistoryModel vod) =>
        {
            await Shell.Current.GoToAsync($"showVideoPage?vod_id={vod.vod_id}&vod_play_from={vod.vod_play_from}");
        });

        DeleteHisVideoCommand = new Command<VideoHistoryModel>(async (VideoHistoryModel vod) =>
        {
            await this.videoDatabase.DeleteHisVideoAsync(vod);
            HisVideoList.Remove(vod);
            OnPropertyChanged(nameof(HisVideoList));
        });
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var hisVideoList = await this.videoDatabase.GetHisVideoListAsync();
        if (hisVideoList != null)
        {
            foreach (var item in hisVideoList)
            {
                HisVideoList.Add(item);
            }
            OnPropertyChanged(nameof(HisVideoList));
        }
    }
    public async void GetHisVideoListAsync()
    {
        var hisVideoList = await this.videoDatabase.GetHisVideoListAsync();
        if (hisVideoList != null)
        {
            foreach (var item in hisVideoList)
            {
                HisVideoList.Add(item);
            }
            OnPropertyChanged(nameof(HisVideoList));
        }
    }


    private ObservableCollection<SearchVideoListModel> searchResults = new ObservableCollection<SearchVideoListModel>();
    public ObservableCollection<SearchVideoListModel> SearchResults
    {
        get
        {
            return searchResults;
        }
        set
        {
            searchResults = value;
            OnPropertyChanged(nameof(SearchResults));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public ObservableCollection<VideoHistoryModel> HisVideoList { get; set; } = new ObservableCollection<VideoHistoryModel>();
}

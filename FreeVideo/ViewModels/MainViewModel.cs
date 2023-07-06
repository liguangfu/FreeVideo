using CommunityToolkit.Mvvm.Input;
using FreeVideo.Models;
using FreeVideo.Services;
using System.Collections.ObjectModel;

namespace FreeVideo.ViewModels;

public class MainViewModel : BaseViewModel, IQueryAttributable
{
    private ISearchVideoService _searchVideoService;
    private readonly ISearchVideoServiceFactory _searchVideoServiceFactory;
    private readonly IHistoryVideoService _historyVideoService;
    public MainViewModel(ISearchVideoServiceFactory searchVideoServiceFactory, IHistoryVideoService historyVideoService)
    {
        _searchVideoServiceFactory = searchVideoServiceFactory;
        _historyVideoService = historyVideoService;

        _lazyPerformSearchCommand = new Lazy<AsyncRelayCommand<string>>(new AsyncRelayCommand<string>(PerformSearchCommandFunction));

        _lazySelectSearchResultCommand = new Lazy<AsyncRelayCommand<SearchVideoListModel>>(new AsyncRelayCommand<SearchVideoListModel>(SelectSearchResultCommandFunction));

        _lazySelectHisVideoCommand = new Lazy<AsyncRelayCommand<VideoHistoryModel>>(new AsyncRelayCommand<VideoHistoryModel>(SelectHisVideoCommandFunction));

        _lazyDeleteHisVideoCommand = new Lazy<AsyncRelayCommand<VideoHistoryModel>>(new AsyncRelayCommand<VideoHistoryModel>(DeleteHisVideoCommandFunction));
    }
    /// <summary>
    /// 搜索视频
    /// </summary>
    public IAsyncRelayCommand<string> PerformSearchCommand => _lazyPerformSearchCommand.Value;
    private Lazy<AsyncRelayCommand<string>> _lazyPerformSearchCommand;
    public async Task PerformSearchCommandFunction(string query)
    {
        SearchResults.Clear();

        _searchVideoService = _searchVideoServiceFactory.CreateSearchVideoService(Common.SearchSourceEnum.zyk1080);

        SearchResults = new ObservableCollection<SearchVideoListModel>(await _searchVideoService.GetSearchListAsync(query));

        var hisVideoList = await _historyVideoService.GetHisVideoListAsync();
        if (hisVideoList != null)
        {
            foreach (var item in hisVideoList)
            {
                HisVideoList.Add(item);
            }
            OnPropertyChanged(nameof(HisVideoList));
        }
    }

    /// <summary>
    /// 选择搜索结果
    /// </summary>
    public AsyncRelayCommand<SearchVideoListModel> SelectSearchResultCommand => _lazySelectSearchResultCommand.Value;
    private Lazy<AsyncRelayCommand<SearchVideoListModel>> _lazySelectSearchResultCommand;
    public async Task SelectSearchResultCommandFunction(SearchVideoListModel selectItem)
    {
        var hisVideo = await _historyVideoService.GetHisVideoAsync(selectItem.vod_id);

        var searchResult = await _searchVideoService.GetSearchDetailAsync(selectItem.vod_id);
        if (searchResult != null)
        {
            if (hisVideo != null)
            {
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
            await _historyVideoService.SaveHisVideoAsync(hisVideo);
        }

        await Shell.Current.GoToAsync($"showVideoPage?vod_id={selectItem.vod_id}&vod_play_from={selectItem.vod_play_from}");
    }

    /// <summary>
    /// 选择历史视频
    /// </summary>
    public AsyncRelayCommand<VideoHistoryModel> SelectHisVideoCommand => _lazySelectHisVideoCommand.Value;
    private Lazy<AsyncRelayCommand<VideoHistoryModel>> _lazySelectHisVideoCommand;
    public async Task SelectHisVideoCommandFunction(VideoHistoryModel vod)
    {
        var searchResult = await _searchVideoService.GetSearchDetailAsync(vod.vod_id);
        if (searchResult != null)
        {
            vod.show_time = DateTime.Now;
            vod.vod_remarks = searchResult.vod_remarks;
            vod.vod_play_url = searchResult.vod_play_url;
            await _historyVideoService.SaveHisVideoAsync(vod);
        }

        await Shell.Current.GoToAsync($"showVideoPage?vod_id={vod.vod_id}&vod_play_from={vod.vod_play_from}");
    }

    /// <summary>
    /// 删除历史视频
    /// </summary>
    public AsyncRelayCommand<VideoHistoryModel> DeleteHisVideoCommand => _lazyDeleteHisVideoCommand.Value;
    private Lazy<AsyncRelayCommand<VideoHistoryModel>> _lazyDeleteHisVideoCommand;
    public async Task DeleteHisVideoCommandFunction(VideoHistoryModel vod)
    {
        await _historyVideoService.DeleteHisVideoAsync(vod);
        HisVideoList.Remove(vod);
        OnPropertyChanged(nameof(HisVideoList));
    }


    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var hisVideoList = await _historyVideoService.GetHisVideoListAsync();
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

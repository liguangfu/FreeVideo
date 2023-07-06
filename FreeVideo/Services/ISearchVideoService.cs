using FreeVideo.Models;

namespace FreeVideo.Services;

public interface ISearchVideoService
{
    Task<List<SearchVideoListModel>> GetSearchListAsync(string query);

    Task<SearchVideoDetailtModel> GetSearchDetailAsync(string vod_id);
}

public class NullSearchVideoService : ISearchVideoService
{
    public Task<List<SearchVideoListModel>> GetSearchListAsync(string query)
    {
        return Task.FromResult(new List<SearchVideoListModel>());
    }

    public Task<SearchVideoDetailtModel> GetSearchDetailAsync(string vod_id)
    {
        return Task.FromResult<SearchVideoDetailtModel>(null);
    }
}
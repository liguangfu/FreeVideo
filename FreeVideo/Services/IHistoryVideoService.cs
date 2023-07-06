using FreeVideo.Models;

namespace FreeVideo.Services;

public interface IHistoryVideoService
{
    Task<List<VideoHistoryModel>> GetHisVideoListAsync();
    Task<VideoHistoryModel> GetHisVideoAsync(string vod_id);

    Task<int> SaveHisVideoAsync(VideoHistoryModel item);

    Task<int> DeleteHisVideoAsync(VideoHistoryModel item);
}
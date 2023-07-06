using FreeVideo.Common;

namespace FreeVideo.Services;

public interface ISearchVideoServiceFactory
{
    ISearchVideoService CreateSearchVideoService(SearchSourceEnum type);
}

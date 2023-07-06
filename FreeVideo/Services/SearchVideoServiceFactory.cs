using FreeVideo.Common;

namespace FreeVideo.Services;

public class SearchVideoServiceFactory : ISearchVideoServiceFactory
{
    public ISearchVideoService CreateSearchVideoService(SearchSourceEnum type)
    {
        switch (type)
        {
            case SearchSourceEnum.zyk1080:
                return new zyk1080SearchVideoService();
            default:
                return new NullSearchVideoService();
        }
    }
}

using CocoaAniCore.Downloaders.Exceptions;
using VideoDownload.Args;
using VideoDownload.Drivers;
using VideoDownload.Files;
using VideoDownload.Results;

namespace VideoDownload.Tasks;

public class M3U8DownloadTask : HttpDownloadTask
{
    public new M3U8DownloadResult Result => (M3U8DownloadResult)_result;
    public new M3U8DownloadArgs Args => (M3U8DownloadArgs)_args;
    public new Func<M3U8DownloadTask, bool>? Adjuster => (Func<M3U8DownloadTask, bool>)_adjuster!;

    public M3U8DownloadTask(M3U8DownloadArgs args) : base(args)
    {
        //驱动为空 设置默认驱动
        args.Driver = args.Driver ?? M3U8DownloadDriver.Instance;
    }
    public M3U8DownloadTask(HttpDownloadArgs args) : base(args)
    {
        _args = new M3U8DownloadArgs(args); 
        //驱动为空 设置默认驱动
        _args.Driver = args.Driver ?? M3U8DownloadDriver.Instance;
    }

    public new async Task<M3U8DownloadTask> ExecuteAsync()
    {
        return (M3U8DownloadTask)await base.ExecuteAsync();
    }
}
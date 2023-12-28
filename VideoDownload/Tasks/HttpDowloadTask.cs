using System.Diagnostics;
using System.Net.Http.Headers;
using CocoaAniCore.Downloaders.Exceptions;
using VideoDownload.Args;
using VideoDownload.Drivers;
using VideoDownload.Enums;
using VideoDownload.Results;
namespace VideoDownload.Tasks;

public class HttpDownloadTask
{
    protected Func<HttpDownloadTask, bool>? _adjuster;
    protected HttpDownloadArgs _args;
    protected HttpDownloadResult _result;
    protected Task<HttpDownloadTask>? _workTask;

    public HttpDownloadTask(HttpDownloadArgs args)
    {
        _args = args;
        _result = args.CreateResult();
        args.Driver = args.Driver ?? DriverManager.GetDriver("Http");
    }

    public Func<HttpDownloadTask, bool>? Adjuster
    {
        get => _adjuster;
        set => _adjuster = value;
    }
    public HttpDownloadArgs Args => _args;
    public HttpDownloadResult Result => _result;
    public Task<HttpDownloadTask>? WorkTask => _workTask;

    public async Task<HttpDownloadTask> ExecuteAsync()
    {
        if (Args.Driver == null) throw new FileDownloadException("NotFoundDriver");
        if (Adjuster != null)
        {
            return await (_workTask = BaseExecuteOnAdjusterAsync());
        }
        return await (_workTask = Args.Driver.ExecuteAsync(this));
    }
    private async Task<HttpDownloadTask> BaseExecuteOnAdjusterAsync()
    {
        if (Args.Driver == null) throw new FileDownloadException("NotFoundDriver");
        Debug.Assert(Adjuster != null, nameof(Adjuster) + " != null");
        while (Adjuster.Invoke(this))
        {
            //                         任务执行                             结果判断
            if ((await Args.Driver.ExecuteAsync(this))._result.State != DownloadState.Success)
            {
                return this;
            }
        }
        return this;
    }


}


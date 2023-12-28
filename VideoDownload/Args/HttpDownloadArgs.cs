using CocoaAniCore.Downloaders.Events;
using VideoDownload.Drivers.Base;
using VideoDownload.Results;

namespace VideoDownload.Args;
public class HttpDownloadArgs
{
    public HttpDownloadArgs()
    {
        
    }
    public abstract class Builder
    {

        protected HttpDownloadArgs _args;

        public Builder SetTaskName(string name)
        {
            _args.TaskName = name;
            return this;
        }

        public Builder SetUrl(string url)
        {
            _args.Url = url;
            return this;
        }

        public Builder SetRange(long start, long end)
        {
            _args.RangeStart = start;
            _args.RangeEnd = end;
            return this;
        }

        public Builder SetSavePath(string savePath)
        {
            _args.SavePath = savePath;
            return this;
        }

        public Builder SetTaskCount(int count)
        {
            _args.TaskCount = count;
            return this;
        }

        public Builder SetCustomArg(object customArg)
        {
            _args.CustomArg = customArg;
            return this;
        }

        public Builder SetDriver(IHttpFileDownloadDriver driver)
        {
            _args.Driver = driver;
            return this;
        }

        public Builder SetDownloadSuccessEventHandle(EventHandler<DownloadStateChangeEventArgs> handle)
        {
            _args.DownloadSuccess += handle;
            return this;
        }

        public Builder SetDownloadErrorEventHandle(EventHandler<DownloadStateChangeEventArgs> handle)
        {
            _args.DownloadError += handle;
            return this;
        }

        public Builder SetDownloadStartEventHandle(EventHandler<DownloadStateChangeEventArgs> handle)
        {
            _args.DownloadStart += handle;
            return this;
        }

        public Builder SetDownloadEndEventHandle(EventHandler<DownloadStateChangeEventArgs> handle)
        {
            _args.DownloadEnd += handle;
            return this;
        }

        public Builder SetProgressUpdateEventHandle(EventHandler<DownloadStateChangeEventArgs> handle)
        {
            _args.ProgressUpdate += handle;
            return this;
        }

        public HttpDownloadArgs Build()
        {
            return _args;
        }
    }
    /// <summary>
    /// 定制参数
    /// </summary>
    public object CustomArg = DownloadConfig.DefaultCustomArg;
    /// <summary>
    /// 下载任务名称
    /// </summary>
    public string TaskName = DownloadConfig.DefaultTaskName;
    /// <summary>
    /// 下载URL地址
    /// </summary>
    public string Url = DownloadConfig.UndefinedUrl;
    /// <summary>
    /// 保存路径
    /// </summary>
    public string SavePath = DownloadConfig.SaveOnMemory;
    /// <summary>
    /// 使用的任务数量
    /// </summary>
    public int TaskCount = DownloadConfig.DefaultTaskCount;
    /// <summary>
    /// 下载的开始范围Byte
    /// </summary>
    public long RangeStart = 0;
    /// <summary>
    /// 下载的结束范围Byte
    /// </summary>
    public long RangeEnd = 0;
    /// <summary>
    /// 错误重试次数
    /// </summary>
    public int ErrorRetries = DownloadConfig.DefaultErrorRetries;
    /// <summary>
    /// 子任务错误重试次数
    /// </summary>
    public int SubTaskErrorRetries = DownloadConfig.DefaultSubTaskErrorRetries;
    /// <summary>
    /// 进度更新触发事件阈值
    /// </summary>
    public float ProgressNotifyThreshold = DownloadConfig.DefaultProgressNotifyThreshold;
    /// <summary>
    /// 子任务通知主任务进度更新 子任务需要达到的进度
    /// </summary>
    public float SubTaskProgressNotifyParentThreshold = DownloadConfig.DefaultSubTaskProgressNotifyParentThreshold;
    /// <summary>
    /// 添加的Http请求头
    /// </summary>
    public Dictionary<string, string> RequestHeaders = DownloadConfig.DefaultHttpRequestHeaders;
    /// <summary>
    /// 下载使用的驱动
    /// </summary>
    public IHttpFileDownloadDriver? Driver;
    /// <summary>
    /// 下载成功事件
    /// </summary>
    public event EventHandler<DownloadStateChangeEventArgs>? DownloadSuccess;
    /// <summary>
    /// 下载进度更新事件
    /// </summary>
    public event EventHandler<DownloadStateChangeEventArgs>? ProgressUpdate;
    /// <summary>
    /// 下载发生错误事件
    /// </summary>
    public event EventHandler<DownloadStateChangeEventArgs>? DownloadError;
    /// <summary>
    /// 下载开始事件
    /// </summary>
    public event EventHandler<DownloadStateChangeEventArgs>? DownloadStart;
    /// <summary>
    /// 下载结束事件
    /// </summary>
    public event EventHandler<DownloadStateChangeEventArgs>? DownloadEnd;

    internal void OnProgressUpdate(HttpDownloadResult downloadResult)
    {
        ProgressUpdate?.Invoke(Driver, new DownloadStateChangeEventArgs(downloadResult));
    }

    internal void OnDownloadError(HttpDownloadResult downloadResult)
    {
        DownloadError?.Invoke(Driver, new DownloadStateChangeEventArgs(downloadResult));
    }

    internal void OnDownloadSuccess(HttpDownloadResult downloadResult)
    {
        DownloadSuccess?.Invoke(Driver, new DownloadStateChangeEventArgs(downloadResult));
    }

    internal void OnDownloadStart(HttpDownloadResult downloadResult)
    {
        DownloadStart?.Invoke(Driver, new DownloadStateChangeEventArgs(downloadResult));
    }

    internal void OnDownloadEnd(HttpDownloadResult downloadResult)
    {
        DownloadEnd?.Invoke(Driver, new DownloadStateChangeEventArgs(downloadResult));
    }

    public bool Reset()
    {
        TaskName = DownloadConfig.DefaultTaskName;
        TaskCount = DownloadConfig.DefaultTaskCount;
        Driver = DownloadConfig.DefaultDriver;
        Url = DownloadConfig.UndefinedUrl;
        SavePath = DownloadConfig.SaveOnMemory;
        CustomArg = DownloadConfig.DefaultCustomArg;
        RangeStart = 0;
        RangeEnd = 0;
        DownloadError = null;
        DownloadSuccess = null;
        DownloadStart = null;
        DownloadEnd = null;
        ProgressUpdate = null;
        return true;
    }

    public virtual HttpDownloadResult CreateResult()
    {
        return new HttpDownloadResult(this);
    }
}
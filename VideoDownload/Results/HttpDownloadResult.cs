using CocoaAniCore.Downloaders.Exceptions;
using VideoDownload.Args;
using VideoDownload.Enums;
using VideoDownload.Tasks;
using System.Net.Http.Headers;

namespace VideoDownload.Results;

public class HttpDownloadResult
{

    private float _progress = 0;
    public object CustomResult { get; internal set; } = DownloadConfig.DefaultCustomResult;
    public float LastProgress { get; set; }

    public HttpResponseHeaders? ResponseHeaders;
    /// <summary>
    /// 用于查看进度 和触发进度更新事件
    /// </summary>
    public float Progress
    {
        get => _progress;
        internal set
        {
            LastProgress = Progress;
            _progress = value;

            Args.OnProgressUpdate(this); //触发事件
            if (Progress < 1) return;
            State = DownloadState.Success; //触发事件
        }
    }

    public long FileSize { get; internal set; }

    public Stream? Stream { get; internal set; }

    private DownloadState _state = DownloadState.UnInit;

    /// <summary>
    /// 用于查看任务状态 和触发Start，Success，Error，End，事件
    /// </summary>
    public DownloadState State
    {
        get => _state;
        set
        {
            switch (value)
            {
                case DownloadState.Start://由Driver 设置
                    Args.OnDownloadStart(this);
                    break;

                case DownloadState.Success: //由Progress属性管理
                    Args.OnDownloadSuccess(this);
                    break;

                case DownloadState.Error: //由Error属性管理
                    Args.OnDownloadError(this);
                    break;

                case DownloadState.End: //由Error/Progress属性管理
                    Args.OnDownloadEnd(this);
                    break;
            }
            _state = value;
        }
    }

    private FileDownloadException? _error;

    public FileDownloadException? Error
    {
        get => _error;
        set
        {
            _error = value;
            State = DownloadState.Error;
        }
        //触发事件
    }

    public bool IsError => State == DownloadState.Error;

    public Dictionary<int, HttpDownloadTask> SubTaskDict { get; internal set; } = new Dictionary<int, HttpDownloadTask>();
    public HttpDownloadArgs Args { get; private set; }
    public int ErrorRetriesCounter = 0;
    public HttpDownloadResult(HttpDownloadArgs args)
    {
        Args = args;
        FileSize = 0;
        _progress = 0;
        State = DownloadState.UnInit;
    }

    internal bool InitStream()
    {
        if (State == DownloadState.Start) return true;
        try
        {
            if (Args.SavePath == DownloadConfig.SaveOnMemory)
            {
                Stream = new MemoryStream(DownloadConfig.DefaultMemoryStreamSize);
            }
            else
            {
                if (File.Exists(Args.SavePath))
                {
                    File.Delete(Args.SavePath);
                }
                //if (!Directory.Exists(Args.SavePath))
                //{
                //    Directory.CreateDirectory(Args.SavePath);
                //}
                Stream= new FileStream(Args.SavePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            }
        }
        catch (Exception ex)
        {
            this.Error = new FileDownloadException($"返回结果初始化 错误 本地文件流创建失败！因为：{ex.Message}", ex);
            return false;
        }
        //设置状态为开始
        State = DownloadState.Start;
        return true;

        //if (Args.TaskCount <= 1) return true;
        ////多线程配置
        ////创建子任务配置信息
        //for (int i = 0; i < Args.TaskCount; i++)
        //{
        //    var subTaskInfo = new HttpDownloadTask(
        //        $"sub-{Args.TaskName}-{i}",
        //        $"{DownloadConfig.TempDir}/sub-{Args.TaskName}-{i}.CocoaAni",
        //        Args.Url);
        //    subTaskInfo.ProgressUpdate += (o, e) =>
        //    {
        //        this.Progress += e.Progress - e.LastProgress / Args.TaskCount;
        //    };
        //    SubTaskDict[i] = subTaskInfo;
        //}

        //return true;
    }

    public string ReadToString()
    {
        return State != DownloadState.Success ? "下载 任务未完成" : new StreamReader(Stream!).ReadToEnd();
    }

    public bool ResetAll()
    {
        if (State != DownloadState.Error || State !=DownloadState.Success || State != DownloadState.End) return false;
        Progress = 0;
        LastProgress = 0;
        FileSize = 0;
        Stream!.Close();
        Stream = null;
        //删除临时文件
        if (Args.SavePath.StartsWith(DownloadConfig.TempDir))
        {
            if (File.Exists(Args.SavePath))
            {
                File.Delete(Args.SavePath);
            }
        }
        State = DownloadState.UnInit;
        return true;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoaAniCore.Downloaders.Enums;
using VideoDownload.Drivers;
using VideoDownload.Results;

namespace VideoDownload.Args;

public class M3U8DownloadArgs : HttpDownloadArgs
{
    /// <summary>
    /// 是否合并TS
    /// </summary>
    public bool IsMergeTs = DownloadConfig.DefaultM3U8IsMergeTs;
    /// <summary>
    /// 是否保存TS片段
    /// </summary>
    public bool IsSaveSubTs = DownloadConfig.DefaultM3U8IsSaveSubTs;

    public M3U8DownloadArgs()
    {
        Driver=M3U8DownloadDriver.Instance;
    }
    public M3U8DownloadArgs(HttpDownloadArgs args)
    {
        this.RangeStart = args.RangeStart;
        this.RangeEnd = args.RangeEnd;
        this.SavePath = args.SavePath;
        this.Url = args.Url;
        this.TaskName=args.TaskName;
        this.CustomArg = args.CustomArg;
        this.RequestHeaders = args.RequestHeaders;
        this.TaskCount = args.TaskCount;
        Driver = DriverManager.GetDriver(DownloadFileType.M3U8.ToString());
    }

    public override M3U8DownloadResult CreateResult()
    {
        return new M3U8DownloadResult(this);
    }

    public new class Builder:HttpDownloadArgs.Builder
    {
        protected new M3U8DownloadArgs _args;

        public Builder SetIsMergeTs(bool value)
        {
            _args.IsMergeTs = value;
            return this;
        }

        public Builder SetIsSaveSubTs(bool value)
        {
            _args.IsSaveSubTs = value;
            return this;
        }



    }

}
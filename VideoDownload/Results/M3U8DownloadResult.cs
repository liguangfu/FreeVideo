using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoDownload.Args;
using VideoDownload.Files;

namespace VideoDownload.Results;
public class M3U8DownloadResult:HttpDownloadResult
{
    public M3U8File? M3U8File { get;internal set; }

    public M3U8DownloadResult(M3U8DownloadArgs args) : base(args)
    {

    }
}
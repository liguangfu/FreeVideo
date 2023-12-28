using CocoaAniCore.Downloaders.Exceptions;
using VideoDownload.Args;
using VideoDownload.Enums;
using VideoDownload.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace VideoDownload.Drivers.Base
{
    public interface IHttpFileDownloadDriver
    {
        public static string DriverName => "";
        public  Task<HttpDownloadTask> ExecuteAsync(HttpDownloadTask task);
    }
}

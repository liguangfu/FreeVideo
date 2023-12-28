using VideoDownload.Enums;
using VideoDownload.Results;
using VideoDownload.Tasks;

namespace CocoaAniCore.Downloaders.Events
{
    public class DownloadStateChangeEventArgs
    {
        public HttpDownloadResult DownloadResult { get; private set; }
        public DownloadState State { get; private set; }

        public DownloadStateChangeEventArgs(HttpDownloadResult downloadResult)
        {
            this.State = downloadResult.State;
            this.DownloadResult = downloadResult;
        }
    }
}
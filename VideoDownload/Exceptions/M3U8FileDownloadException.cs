using VideoDownload.Tasks;

namespace CocoaAniCore.Downloaders.Exceptions
{
    internal class M3U8FileDownloadException : FileDownloadException
    {
        public M3U8FileDownloadException(HttpDownloadTask task, string message, Exception caseException) : base(message, caseException)
        {
        }

        public M3U8FileDownloadException(HttpDownloadTask task, string message) : base(message)
        {
        }
    }
}
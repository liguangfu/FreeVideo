namespace CocoaAniCore.Downloaders.Exceptions
{
    public class FileDownloadException : System.Exception
    {
        public FileDownloadException(string message) : base(message)
        {
        }

        public FileDownloadException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
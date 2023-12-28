using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoaAniCore.Downloaders.Exceptions;

namespace VideoDownload.Exceptions
{
    public class FileCheckException : FileDownloadException
    {
        public FileCheckException(string message) : base(message)
        {
        }

        public FileCheckException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

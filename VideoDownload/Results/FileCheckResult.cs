using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoDownload.Exceptions;

namespace VideoDownload.Results
{
    public class FileCheckResult
    {
        public bool IsOk => ErrorMessage == null;
        public bool IsError => ErrorMessage != null;
        public string? ErrorMessage { get;internal set; }
        public FileCheckResult()
        {
            
        }
        public FileCheckResult(string errorMessage)
        {
            this.ErrorMessage = errorMessage;
        }
    }
}

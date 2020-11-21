using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudService_API.Data
{
    public class FilePathSettings
    {
        public string FolderForFiles { get; set; }

        public FilePathSettings() {}

        public FilePathSettings(string folderForFiles)
        {
            FolderForFiles = string.IsNullOrEmpty(folderForFiles) ? "C:\\CloudService\\Files" : folderForFiles;
        }
    }
}

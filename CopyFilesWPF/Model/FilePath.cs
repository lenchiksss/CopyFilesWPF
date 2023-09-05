using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyFilesWPF.Model
{
    public sealed record FilePath
    {
        private string pathTo { get; set; } = string.Empty;

        public string pathFrom { get; set; } = string.Empty;

        public string PathTo
        {
            get => pathTo + "\\" + Path.GetFileName(pathFrom);
            set => pathTo = value;
        }
    }
}
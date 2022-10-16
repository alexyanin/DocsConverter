using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocsConverter.Domain.Models
{
    public class FileData
    {
        public string FileName { get; set; }

        public string Content { get; set; }

        public byte[] Bytes { get; set; }
    }
}

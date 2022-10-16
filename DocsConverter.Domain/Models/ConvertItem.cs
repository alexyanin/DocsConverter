using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocsConverter.Domain.Models
{
    public class ConvertItem
    {
        public FileData SourceFile { get; set; }

        public FileData DestinationFile { get; set; }
    }
}

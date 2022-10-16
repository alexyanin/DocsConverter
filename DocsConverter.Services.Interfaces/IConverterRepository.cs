using DocsConverter.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocsConverter.Services.Interfaces
{
    public interface IConverterRepository
    {
        Task<bool> SaveFileAsync(string fileName, byte[] content);

        Task<bool> ConvertAsync(string browserFetcherFolder, string fromFileName, string toFilePath);

        bool CheckIsReady(string fileName);

        Task<FileData> ReadFileAsync(string fileName);

        Task<bool> CheckNeedConvertAsync(string browserFetcherFolder, string srcPath, string dstPath);
    }
}

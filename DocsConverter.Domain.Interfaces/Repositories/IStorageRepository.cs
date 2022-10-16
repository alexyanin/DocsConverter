using DocsConverter.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocsConverter.Domain.Interfaces.Repositories
{
    public interface IStorageRepository
    {
        Task<bool> SaveFileAsync(FileData fileData);

        Task<FileData> ReadFileAsync(string fileName);

        Task<FileData> ReadFileBytesAsync(string fileName);

        bool FileExists(string fileName);

        List<string> GetFilesFromDirectory(string directoryName);
    }
}

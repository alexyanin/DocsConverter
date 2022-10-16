using DocsConverter.Domain.Interfaces.Repositories;
using DocsConverter.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocsConverter.Infrastructure.Data
{
    public class FilesStorageRepository : IStorageRepository
    {
        private const int defaultBufferSize = 4096;
        private const FileOptions options = FileOptions.Asynchronous | FileOptions.SequentialScan;


        public async Task<bool> SaveFileAsync(FileData fileData)
        {
            using (FileStream sourceStream = new FileStream(fileData.FileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, bufferSize: defaultBufferSize, useAsync: true))
            {
                await sourceStream.WriteAsync(fileData.Bytes, 0, fileData.Bytes.Length);
            };

            return true;
        }

        public async Task<FileData> ReadFileAsync(string fileName)
        {
            var result = new FileData() { FileName = fileName, Content = "" }; 

            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, defaultBufferSize, options))
                using (var reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        result.Content += line;
                    }
                }

            return result;
        }

        public async Task<FileData> ReadFileBytesAsync(string fileName)
        {
            var result = new FileData() { FileName = fileName, Content = "" };

            using (FileStream stream = File.Open(fileName, FileMode.Open))
            {
                result.Bytes = new byte[stream.Length];
                await stream.ReadAsync(result.Bytes, 0, (int)stream.Length);
            }
            return result;
        }

        public bool FileExists(string fileName)
        {
            return File.Exists(fileName);
        }

        public List<string> GetFilesFromDirectory(string directoryName)
        {
            var result = new List<string>();

            var dir = Directory.EnumerateFiles(directoryName);
            foreach (var file in dir)
            {
                result.Add(Path.GetFileName(file));
            }

            return result;
        }
    }
}


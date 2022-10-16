using DocsConverter.Domain.Interfaces.Repositories;
using DocsConverter.Domain.Models;
using DocsConverter.Services.Interfaces;
using Ninject;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DocsConverter.Infrastructure.Service
{
    public class HtmlToPdfConverter : IConverterRepository
    {
        private IStorageRepository Storage { get; set; }

        public HtmlToPdfConverter(IStorageRepository storageRepo)
        {
            Storage = storageRepo;
        }

        public async Task<bool> SaveFileAsync(string fileName, byte[] content)
        {
            var fileData = new FileData { FileName = fileName, Bytes = content };
            await Storage.SaveFileAsync(fileData);

            return true;
        }

        public async Task<bool> ConvertAsync(string browserFetcherFolder, string fromFileName, string toFilePath)
        {
            var fromFile = await Storage.ReadFileAsync(fromFileName);

            var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
            {
                Path = browserFetcherFolder
            });

            var path = Directory.EnumerateDirectories(browserFetcherFolder).FirstOrDefault();
            if (!string.IsNullOrEmpty(path))
                path += "\\chrome-win\\chrome.exe";

            if (!Storage.FileExists(path))
            {
                await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            }            

            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = path
            });
            var page = await browser.NewPageAsync();
            await page.EmulateMediaTypeAsync(MediaType.Screen);
            await page.SetContentAsync(fromFile.Content);
            var pdfContent = await page.PdfStreamAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true
            });

            // long time operation
            //await Task.Delay(10000);

            FileData resultFileData = new FileData() { FileName = toFilePath, Bytes = new byte[pdfContent.Length] };
            
            await pdfContent.ReadAsync(resultFileData.Bytes, 0, (int)pdfContent.Length);

            await Storage.SaveFileAsync(resultFileData);


            return true;
        }

        public bool CheckIsReady(string fileName)
        {
            return Storage.FileExists(fileName);
        }

        public async Task<FileData> ReadFileAsync(string fileName)
        {
            return await Storage.ReadFileBytesAsync(fileName);
        }

        public async Task<bool> CheckNeedConvertAsync(string browserFetcherFolder, string srcPath, string dstPath)
        {
            var srcFiles = Storage.GetFilesFromDirectory(srcPath);
            var dstFiles = Storage.GetFilesFromDirectory(dstPath);
            foreach (var srcFile in srcFiles)
            {
                string ext = Path.GetExtension(srcFile);
                var dstFile = srcFile.Replace(ext, ".pdf");
                if (!dstFiles.Contains(dstFile))
                {
                    try
                    {
                        var task = ConvertAsync(browserFetcherFolder, srcPath + "\\" + srcFile, dstPath + "\\" + dstFile);
                        await task;
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }
            return true;
        }
    }
}

using DocsConverter.Domain.Interfaces.Repositories;
using DocsConverter.Services.Interfaces;
using Ninject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace DocsConverter.WebApi.Controllers
{
    public class ConvertController : ApiController
    {
        public IConverterRepository Converter { get; set; }

        public ConvertController(IConverterRepository converterRepository)
        {
            Converter = converterRepository;
            var browserFetcherFolder = HttpContext.Current.Server.MapPath("~/Browser");
            var srcFolder = HttpContext.Current.Server.MapPath("~/SourceFiles");
            var dstFolder = HttpContext.Current.Server.MapPath("~/DestinationFiles");
            Converter.CheckNeedConvertAsync(browserFetcherFolder, srcFolder, dstFolder);
            Task.Run<bool>(async () => await Converter.CheckNeedConvertAsync(browserFetcherFolder, srcFolder, dstFolder));
        }

        public async Task<HttpResponseMessage>  Post()
        {
            HttpResponseMessage result = null;
            try
            {
                var httpRequest = HttpContext.Current.Request;
                if (httpRequest.Files.Count > 0)
                {
                    var postedFile = httpRequest.Files[0];
                    var browserFetcherFolder = HttpContext.Current.Server.MapPath("~/Browser");
                    var srcFilePath = HttpContext.Current.Server.MapPath("~/SourceFiles/" + postedFile.FileName);
                    var dstFilePath = HttpContext.Current.Server.MapPath("~/DestinationFiles/" + postedFile.FileName);
                    string ext = Path.GetExtension(dstFilePath);
                    dstFilePath = dstFilePath.Replace(ext, ".pdf");

                    var ms = new MemoryStream();
                    var file = httpRequest.Files[0];
                    await file.InputStream.CopyToAsync(ms);
                    byte[] fileContent = ms.ToArray();
                    await Converter.SaveFileAsync(srcFilePath, fileContent);

                    //var postedFile = httpRequest.Files[file];

                    

                    await Converter.ConvertAsync(browserFetcherFolder, srcFilePath, dstFilePath);

                    result = Request.CreateResponse(HttpStatusCode.OK, true);
                }
                else
                {
                    result = Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                result = Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            return result;
        }

        [HttpGet]
        [Route("api/Convert/CheckIsReady")]
        public HttpResponseMessage CheckIsReady(string fileName)
        {
            HttpResponseMessage result = null;
            try
            {
                var dstFilePath = HttpContext.Current.Server.MapPath("~/DestinationFiles/" + fileName);
                string ext = Path.GetExtension(dstFilePath);
                dstFilePath = dstFilePath.Replace(ext, ".pdf");
                var check =  Converter.CheckIsReady(dstFilePath);

                result = Request.CreateResponse(HttpStatusCode.OK, check);
            }
            catch (Exception ex)
            {
                result = Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            return result;
        }

        [HttpGet]
        [Route("api/Convert/DownloadPdfFile")]
        public async Task<HttpResponseMessage> DownloadPdfFile(string srcFileName)
        {
            var dstFilePath = HttpContext.Current.Server.MapPath("~/DestinationFiles/" + srcFileName);
            string ext = Path.GetExtension(dstFilePath);
            dstFilePath = dstFilePath.Replace(ext, ".pdf");

            var file = await Converter.ReadFileAsync(dstFilePath);

            var stream = new MemoryStream(file == null || file.Bytes == null ? new byte[0] : file.Bytes);


            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StreamContent(stream);

            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = Path.GetFileName(dstFilePath)
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return response;
        }

        [HttpGet]
        [Route("api/Convert/Test")]
        public HttpResponseMessage Test()
        {
           return Request.CreateResponse(HttpStatusCode.OK, HttpStatusCode.OK);
        }
    }
}

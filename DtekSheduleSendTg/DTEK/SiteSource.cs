﻿using DtekSheduleSendTg.Abstraction;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DtekSheduleSendTg.DTEK
{
    public class SiteSource(ILogger logger, string site, string region) : ISiteSource
    {
        public string GetSource()
            => GetSource(0);

        public string GetSource(int attempt)
        {
            logger.LogInformation("Start Get site source");

            HttpClient httpClient = new(new RedirectHandler(new HttpClientHandler()));

            var source = httpClient.GetStringAsync($"{site}/ua/shutdowns").Result;

            if (source.Contains("Request unsuccessful.") && attempt < 5) 
            {
                Thread.Sleep(10000);
                return GetSource(++attempt);
            }

            logger.LogInformation("End Get site source");

            return source;
        }


        public string StorePicFromUrl(string url)
        {
            var fullUrl = $"{site}{url}";

            logger.LogInformation("Start StorePicFromUrl");

           

            using WebClient client = new();

            var fileName = fullUrl.Split('/').LastOrDefault();

            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            var dir = Path.Combine(Environment.CurrentDirectory, "WorkDir", region, "pict");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            fileName = Path.Combine(dir, fileName);
            var tagFileName = $"{fileName}.tag";

            var tag = GetRemoteFileETag(fullUrl);
            logger.LogInformation("StorePicFromUrl tag {0}", tag);

            if (File.Exists(fileName))
            {
                logger.LogInformation("File exists {0}", fileName);
                logger.LogInformation("Check tag {0}", tagFileName);
                if (File.Exists(tagFileName))
                { 
                    var prevTag = File.ReadAllText(tagFileName);
                    if (string.Equals(prevTag, tag))
                        return string.Empty;
                    else
                    {
                        File.Delete(fileName);
                        File.Delete(tagFileName);
                    }
                }
            }

            File.WriteAllText(tagFileName, tag);

            try
            {
                logger.LogInformation("Try Download {0} to file {1}", url, fileName);
                client.DownloadFile(fullUrl, fileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DownloadFile");
                return string.Empty;
            }

            logger.LogInformation("StorePicFromUrl file : {0}", fileName);

            logger.LogInformation("End StorePicFromUrl");

            return fileName;
        }

        static string GetRemoteFileETag(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "HEAD";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                return response.Headers["ETag"];
            }
        }

    }

    public class RedirectHandler : DelegatingHandler
    {
        public RedirectHandler(HttpMessageHandler innerHandler) => InnerHandler = innerHandler;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var responseMessage = await base.SendAsync(request, cancellationToken);

            if (responseMessage is { StatusCode: HttpStatusCode.Redirect, Headers: { Location: { } } })
            {
                request = new HttpRequestMessage(HttpMethod.Get, responseMessage.Headers.Location);
                responseMessage = await base.SendAsync(request, cancellationToken);
            }

            return responseMessage;
        }
    }
}

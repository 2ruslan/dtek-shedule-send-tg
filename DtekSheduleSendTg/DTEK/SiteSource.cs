using DtekSheduleSendTg.Abstraction;
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

            var dir = Path.Combine(Environment.CurrentDirectory, "WorkDir", region, "pict");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var tag = GetRemoteFileETag(fullUrl).Trim('"');
            logger.LogInformation("StorePicFromUrl current tag {0}", tag);

            var ext = fullUrl.Split('.')[^1];
            var fileName = string.IsNullOrEmpty(tag)
                ? Path.Combine(dir, fullUrl.Split('/').LastOrDefault())
                : Path.Combine(dir, $"{tag}.{ext}"); 

            using WebClient client = new();

            if (File.Exists(fileName))
            {
                logger.LogInformation("StorePicFromUrl file exists {0}", fileName);
                return fileName;
            }

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

        string GetRemoteFileETag(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "HEAD";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    return response.Headers["ETag"];
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetRemoteFileETag" );
            }

            return string.Empty;
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

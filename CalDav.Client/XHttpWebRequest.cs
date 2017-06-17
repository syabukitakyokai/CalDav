using CalCli.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

namespace CalDav.Client
{
    public class XHttpWebRequest : IXHttpWebRequest
    {
        public string Authorization { get; set; }
        public string UserAgent { get; set; }
        public RequestHeaders RequestHeaders { get; } = new RequestHeaders();

        public NetworkCredential Credentials { get; set; }

        public XHttpWebResponse Request(Uri url, string method, string contentType, string requestContent)
        {
            var httpClientHandler = new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.GZip,
                //CookieContainer = false,
                UseCookies = false,
                UseDefaultCredentials = false,
            };

            if (Credentials != null)
            {
                httpClientHandler.Credentials = Credentials;
            }
            
            using (var httpClient = new HttpClient(httpClientHandler))
            {
                if (UserAgent != null)
                {
                    httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(UserAgent);
                }

                if (Authorization != null)
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", Authorization);
                }

                foreach (var item in RequestHeaders)
                {
                    httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                }

                var httpRequest = new HttpRequestMessage()
                {
                    Method = new System.Net.Http.HttpMethod(method.ToUpper()),
                    RequestUri = url,
                };

                if (requestContent != null)
                {
                    httpRequest.Content = new StringContent(requestContent, Encoding.UTF8, contentType);
                }

                var response = httpClient.SendAsync(httpRequest).GetAwaiter().GetResult();
                var responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                var responseHeaders = new ResponseHeaders();
                foreach(var h in response.Headers)
                {
                    var val = String.Join(",", h.Value.ToArray());
                    responseHeaders.Add(h.Key, val);
                }
                foreach (var h in response.Content.Headers)
                {
                    var val = String.Join(",", h.Value.ToArray());
                    responseHeaders.Add(h.Key, val);
                }


                var result = new XHttpWebResponse(
                    response.StatusCode,
                    responseContent,
                    responseHeaders
                    );

                // response.EnsureSuccessStatusCode();

                return result;
            }
        }
        public void Dispose()
        {
        }
    }
}

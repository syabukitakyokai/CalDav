using System;
using System.Net;
using CalCli.API;

namespace CalCli
{
    public class BasicConnection : IConnection, ICredentials
    {
        private string username;
        private string password;

        public BasicConnection(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        public IXHttpWebRequest Authorize(IXHttpWebRequest request)
        {
            if (request.Credentials != null)
            {
                //req.Credentials = credentials;
                var b64 = username + ":" + password;
                b64 = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(b64));
                request.Authorization = "Basic " + b64;
            }

            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
            // ((HttpWebRequest)request).UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
            return request;
        }

        public NetworkCredential GetCredential(Uri uri, string authType)
        {
            return new NetworkCredential(username, password);
        }
    }
}
using System.Net;
using CalCli.API;

namespace CalCli.Connections
{
    public class GoogleConnection : IConnection
    {
        private string token;

        public GoogleConnection(string token)
        {
            this.token = token;
        }

        public IXHttpWebRequest Authorize(IXHttpWebRequest request)
        {
            request.Authorization = "Bearer " + token;
            return request;
        }

        public IXHttpWebRequest CreateHttpWebRequest()
        {
            return new XHttpWebRequest();
        }

    }
}

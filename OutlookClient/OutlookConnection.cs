using System.Net;
using CalCli.API;
//using Microsoft.Experimental.IdentityModel.Clients.ActiveDirectory;

namespace CalCli.Connections
{
    public class OutlookConnection : IConnection
    {
        public IXHttpWebRequest Authorize(IXHttpWebRequest request)
        {
            return request;
        }
    }
}

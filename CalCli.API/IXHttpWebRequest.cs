using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CalCli.API
{
    public interface IXHttpWebRequest : IDisposable
    {
        string Authorization { get; set; }

        string UserAgent { get; set; }

        Dictionary<string, string> RequestHeaders { get; }

        NetworkCredential Credentials { get; set; }

    }
}

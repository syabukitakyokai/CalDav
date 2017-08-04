using System;
using System.Net;
using System.Xml.Linq;
using CalCli.API;
using System.Collections.Generic;

namespace CalDav.Client {
	public class Common {
        private IConnection connection;

        public Common(IConnection connection)
        {
            this.connection = connection;
        }

        public XHttpWebResponse Request(Uri url, string method, XDocument content, NetworkCredential credentials = null, System.Collections.Generic.Dictionary<string, string> headers = null) {
			return Request(url, method, content.Root, credentials, headers);
		}
		public XHttpWebResponse Request(Uri url, string method, XElement content, NetworkCredential credentials = null, System.Collections.Generic.Dictionary<string, string> headers = null) {

            return Request(url, method, "text/xml", content.ToString(), credentials, headers);
		}

		public XHttpWebResponse Request(Uri url, string method = "GET", string contentType = null, string requestContent = null, NetworkCredential credentials = null, System.Collections.Generic.Dictionary<string, string> headers = null) {
            var req = connection.CreateHttpWebRequest();
            req.Credentials = credentials;
            connection.Authorize(req);
			// req.Method = method.ToUpper();

			//Dear .NET, please don't try to do things for me.  kthxbai
			// System.Net.ServicePointManager.Expect100Continue = false;

			if (headers != null)
				foreach (var header in headers) {
					var value = Convert.ToString(header.Value);
                    if (header.Key.Is("User-Agent"))
                    {
                        req.UserAgent = value;
                    }
                    else
                    {
                        req.RequestHeaders[header.Key] = value;
                    }
                }

            connection.Authorize(req);

            return req.Request(url, method, contentType, requestContent);
		}

		private System.Net.HttpWebResponse GetResponse(System.Net.HttpWebRequest req) {
			try {
                var responseAsyncResult = req.BeginGetResponse(null, null);
                var returnValue = req.EndGetResponse(responseAsyncResult);
				return (System.Net.HttpWebResponse)returnValue;
			} catch (System.Net.WebException wex) {
				return (System.Net.HttpWebResponse)wex.Response;
			}
		}

		public static void Serialize(System.IO.Stream stream, Event e, System.Text.Encoding encoding = null) {
			var ical = new CalDav.Calendar();
			ical.Events.Add(e);
			Serialize(stream, ical, encoding);
		}

		public static void Serialize(System.IO.Stream stream, CalDav.Calendar ical, System.Text.Encoding encoding = null) {
            //System.IO.StreamWriter sr = new System.IO.StreamWriter(stream);
            //sr.WriteLine("BEGIN: VCALENDAR");
            //sr.WriteLine("VERSION:2.0");
            //sr.WriteLine("PRODID: CalCli");
            //sr.WriteLine("BEGIN:VEVENT");
            //var enumer = ical.Events.GetEnumerator();
            //enumer.MoveNext();
            //sr.WriteLine("UID:"+enumer.Current.UID);
            //sr.WriteLine("DTSTAMP:20060712T182145Z");
            //sr.WriteLine("DTSTART:20060714T170000Z");
            //sr.WriteLine("DTEND:20060715T040000Z");
            //sr.WriteLine("SUMMARY:Bastille Day Party");
            //sr.WriteLine("END:VEVENT");
            //sr.WriteLine("END:VCALENDAR");
            //return 
			var serializer = new CalDav.Serializer();
			serializer.Serialize(stream, ical, encoding);
		}
	}
}

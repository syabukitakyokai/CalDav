using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CalDav.Client;
using CalCli.API;

namespace CalDav.Client {
	public class Server : IServer {
        private Common Common;
        private IConnection connection;
		public Uri Url { get; set; }
		public System.Net.NetworkCredential Credentials { get; set; }
		public Server(string url, IConnection connection, string username = null, string password = null)
			: this(new Uri(url), connection, username, password) { }

        private HashSet<string> _Options;
        // private string v;
        // private IConnection connection;

        public Server(Uri url, IConnection connection, string username = null, string password = null) {
            this.connection = connection;
            Common = new Common(connection);
            Url = url;
			if (username != null && password != null) {
				Credentials = new System.Net.NetworkCredential(username, password);
			}
			_Options = GetOptions();
            completeUrl();
		}

        private void completeUrl()
        {
            var xcollectionset = CalDav.Common.xDav.GetName("current-user-principal");
            var result = Common.Request(Url, "propfind", new XDocument(
                    new XElement(CalDav.Common.xDav.GetName("propfind"),
                        new XElement(CalDav.Common.xDav.GetName("prop"),
                            new XElement(xcollectionset)
                            )

                        )
                ), Credentials, new System.Collections.Generic.Dictionary<string, string> { { "Depth", "0" } });
            
            var xdoc = XDocument.Parse(result.ResponseContent);
            Uri userprincipal = Url;
            foreach(XNode node in xdoc.Descendants(xcollectionset))
            {
                foreach (XElement href in xdoc.Descendants(CalDav.Common.xDav.GetName("href")))
                {
                    if (href.Parent == node)
                    {
                        userprincipal = new Uri(Url, href.Value);
                    }
                }
            }
            // get homeset
            xcollectionset = CalDav.Common.xCalDav.GetName("calendar-home-set");
            result = Common.Request(userprincipal, "propfind", new XDocument(
                    new XElement(CalDav.Common.xDav.GetName("propfind"),
                        new XElement(CalDav.Common.xDav.GetName("prop"),
                            new XElement(xcollectionset)
                            )

                        )
                ), Credentials, new System.Collections.Generic.Dictionary<string, string> { { "Depth", "0" } });
            
            xdoc = XDocument.Parse(result.ResponseContent);
            var hrefs = xdoc.Descendants(xcollectionset).SelectMany(x => x.Descendants(CalDav.Common.xDav.GetName("href")));
            Url = new Uri(Url, hrefs.First().Value);
        }

        public HashSet<string> Options {
			get {
				if (_Options == null)
					lock (this)
						if (_Options == null)
							_Options = GetOptions();
				return _Options;
			}
		}

        public IConnection Connection
        {
            get
            {
                return connection;
            }

            set
            {
                connection = value;
                Common = new Common(connection);
            }
        }

        private HashSet<string> GetOptions() {
			var result = Common.Request(Url, "options", credentials: Credentials);
			if (result.ResponseHeaders["WWW-Authenticate"] != null)
				throw new Exception("Authentication is required");
            if (result.ResponseHeaders["DAV"] == null)
                throw new Exception("This does not appear to be a valid CalDav server");
            var dav = result.ResponseHeaders["DAV"];
			if (!dav.Contains("calendar-access"))
				throw new Exception("This does not appear to be a valid CalDav server");
			return new HashSet<string>((result.ResponseHeaders["Allow"] ?? string.Empty).ToUpper().Split(',').Select(x => x.Trim()).Distinct(), StringComparer.OrdinalIgnoreCase);
		}

		public void CreateCalendar(string name) {
			if (!Options.Contains("MKCALENDAR"))
				throw new Exception("This server does not support creating calendars");
			var result = Common.Request(new Uri(Url, name), "mkcalendar", credentials: Credentials);
			if (result.HttpStatusCode != System.Net.HttpStatusCode.Created)
				throw new Exception("Unable to create calendar");
		}

		public Calendar[] GetCalendars() {
            var xcollectionset = CalDav.Common.xCalDav.GetName("calendar-home-set");
            var result = Common.Request(Url, "propfind", new XDocument(
                    new XElement(CalDav.Common.xDav.GetName("propfind"),
                        //new XElement(CalDav.Common.xDav.GetName("allprop")//,
                        //    //new XElement(xcollectionset)
                        //    )
                        new XElement(CalDav.Common.xDav.GetName("prop"),
                            new XElement(CalDav.Common.xDav.GetName("resourcetype")),
                            //new XElement(CalDav.Common.xDav.GetName("displayname")),
                            // new XElement(CalDav.Common.xCalendarServer.GetName("getctag")),
                            new XElement(CalDav.Common.xCalDav.GetName("supported-calendar-component-set"))
                            )
                        )
                ), Credentials, new System.Collections.Generic.Dictionary<string, string> { { "Depth", "1" } });
            
            if (string.IsNullOrEmpty(result.ResponseContent))
				return new[]{
					 new Calendar(Common) { Url =  Url, Credentials = Credentials }
				};

			var xdoc = XDocument.Parse(result.ResponseContent);
            var responses = xdoc.Descendants(CalDav.Common.xDav.GetName("response"));
            List<Calendar> calendars = new List<Calendar>();
            // var hrefs = xdoc.Descendants(CalDav.Common.xDav.GetName("href"));

            var attributeName = XName.Get("name");
            foreach (XElement response in responses)
            {
                // Resourcetype calendar
                if(response.Descendants(CalDav.Common.xCalDav.GetName("calendar")).Count() > 0)
                {
                    if (response.Descendants(CalDav.Common.xCalDav.GetName("comp")).Where(d => d.Attribute(attributeName).Value == "VEVENT").Count() > 0)
                    {
                        string href = response.Descendants(CalDav.Common.xDav.GetName("href")).First().Value;
                        calendars.Add(new Calendar(Common, new Uri(Url, href), Credentials));
                    }
                }
            }
            return calendars.ToArray();
		}


		public bool Supports(string option) {
			return Options.Contains(option);
		}

        ICalendar[] IServer.GetCalendars()
        {
            return GetCalendars();
        }

        public void CreateCalendar(ICalendar calendar)
        {
            Calendar cal = (Calendar)calendar;
            CreateCalendar(cal.Name);
        }
    }
}

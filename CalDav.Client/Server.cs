using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using CalCli.API;

namespace CalDav.Client {
	public class Server : IServer {
        private Common _common;
        private IConnection _connection;
		public Uri Url { get; set; }
		public NetworkCredential Credentials { get; set; }
		public Server(string url, IConnection connection, string username = null, string password = null)
			: this(new Uri(url), connection, username, password) { }

        private HashSet<string> _options;

        public Server(Uri url, IConnection connection, string username = null, string password = null) {
            _connection = connection;
            _common = new Common(connection);
            Url = url;
			if (username != null && password != null) {
				Credentials = new NetworkCredential(username, password);
			}
			_options = GetOptions();
            CompleteUrl();
		}

        private void CompleteUrl()
        {
            var xcollectionset = CalDav.Common.xDav.GetName("current-user-principal");
            var result = _common.Request(Url, "propfind", new XDocument(
                    new XElement(CalDav.Common.xDav.GetName("propfind"),
                        new XElement(CalDav.Common.xDav.GetName("prop"),
                            new XElement(xcollectionset)
                            )

                        )
                ), Credentials, new Dictionary<string, string> { { "Depth", "0" } });
            
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
            result = _common.Request(userprincipal, "propfind", new XDocument(
                    new XElement(CalDav.Common.xDav.GetName("propfind"),
                        new XElement(CalDav.Common.xDav.GetName("prop"),
                            new XElement(xcollectionset)
                            )

                        )
                ), Credentials, new Dictionary<string, string> { { "Depth", "0" } });
            
            xdoc = XDocument.Parse(result.ResponseContent);
            var hrefs = xdoc.Descendants(xcollectionset).SelectMany(x => x.Descendants(CalDav.Common.xDav.GetName("href")));
            Url = new Uri(Url, hrefs.First().Value);
        }

        public HashSet<string> Options {
			get {
				if (_options == null)
					lock (this)
						if (_options == null)
							_options = GetOptions();
				return _options;
			}
		}

        public IConnection Connection
        {
            get
            {
                return _connection;
            }

            set
            {
                _connection = value;
                _common = new Common(_connection);
            }
        }

        private HashSet<string> GetOptions() {
			var result = _common.Request(Url, "options", credentials: Credentials);
			if (result.ResponseHeaders["WWW-Authenticate"] != null)
				throw new Exception("Authentication is required");
            if (result.ResponseHeaders["DAV"] == null)
                throw new Exception("This does not appear to be a valid CalDav server");
            var dav = result.ResponseHeaders["DAV"];
			if (!dav.Contains("calendar-access"))
				throw new Exception("This does not appear to be a valid CalDav server");
			return new HashSet<string>((result.ResponseHeaders["Allow"] ?? string.Empty).ToUpper().Split(',').Select(x => x.Trim()).Distinct(), StringComparer.OrdinalIgnoreCase);
		}

	    protected XHttpWebResponse MkCalendar(string name, string id) {
            return _common.Request(new Uri(Url, id), "mkcalendar", credentials: Credentials);
        }


        protected XHttpWebResponse MkCol(string name, string id)
        {
            var resourcetype = CalDav.Common.xDav.Element("resourcetype",
                CalDav.Common.xDav.Element("collection"),
                    CalDav.Common.xCalDav.Element("calendar")
            );

            var comp = CalDav.Common.xCalDav.Element("comp");
            comp.SetAttributeValue("name", "VEVENT");

            var supportedComponentSet = CalDav.Common.xCalDav.Element(
                    "supported-calendar-component-set",
                    comp
                );
            var displayName = CalDav.Common.xDav.Element("displayname", name);

            return _common.Request(new Uri(Url, id), "MKCOL",
                CalDav.Common.xDav.Element("mkcol",
                    CalDav.Common.xDav.Element("set",
                        CalDav.Common.xDav.Element("prop",
                            resourcetype,
                            supportedComponentSet,
                            displayName
                        )
                    )
                ),
                credentials: Credentials);

        }

        public void CreateCalendar(string name)
        {
            var id = NameToId(name);

            XHttpWebResponse response;

            if (Options.Contains("MKCALENDAR"))
            {
                response = MkCalendar(name, id);
            }
            else if (Options.Contains("MKCOL"))
            {
                response = MkCol(name, id);
            }
            else
                throw new Exception("This server does not support creating calendars");

            if (response.HttpStatusCode != HttpStatusCode.Created)
                throw new Exception("Unable to create calendar");
        }

        private static string NameToId(string name)
        {
            return name.Replace(" ", string.Empty).Replace("(", "-").Replace(")", "-");
        }

        public Calendar GetCalendar(string name)
        {
            var id = NameToId(name);
            var result = _common.Request(new Uri(Url, id), "propfind", new XDocument(
                    new XElement(CalDav.Common.xDav.GetName("propfind"),
                        new XElement(CalDav.Common.xDav.GetName("allprop")//,
                            )

                        )
                ), Credentials, new Dictionary<string, string> { { "Depth", "0" } });

            if (result.HttpStatusCode == HttpStatusCode.NotFound || string.IsNullOrEmpty(result.ResponseContent))
                return null;

            var xdoc = XDocument.Parse(result.ResponseContent);
            var responses = xdoc.Descendants(CalDav.Common.xDav.GetName("response"));
            List<Calendar> calendars = new List<Calendar>();

            foreach (XElement response in responses)
            {
                if (response.Descendants(CalDav.Common.xCalDav.GetName("calendar")).Count() > 0)
                {
                    string href = response.Descendants(CalDav.Common.xDav.GetName("href")).First().Value;
                    calendars.Add(new Calendar(_common, new Uri(Url, href), Credentials));
                }
            }
            return calendars.FirstOrDefault();
        }


        public Calendar[] GetCalendars() {
            var result = _common.Request(Url, "propfind", new XDocument(
                    new XElement(CalDav.Common.xDav.GetName("propfind"),
                        new XElement(CalDav.Common.xDav.GetName("prop"),
                            new XElement(CalDav.Common.xDav.GetName("resourcetype")),
                            new XElement(CalDav.Common.xCalDav.GetName("supported-calendar-component-set"))
                            )
                        )
                ), Credentials, new Dictionary<string, string> { { "Depth", "1" } });
            
            if (string.IsNullOrEmpty(result.ResponseContent))
				return new[]{
					 new Calendar(_common) { Url =  Url, Credentials = Credentials }
				};

			var xdoc = XDocument.Parse(result.ResponseContent);
            var responses = xdoc.Descendants(CalDav.Common.xDav.GetName("response"));
            var calendars = new List<Calendar>();
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
                        calendars.Add(new Calendar(_common, new Uri(Url, href), Credentials));
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

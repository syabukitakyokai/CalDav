using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using CalCli.API;
using System.IO;

namespace CalDav.Client {
	public class Calendar : ICalendar {
		public Uri Url { get; set; }
		public NetworkCredential Credentials { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
        public Common common { get; set; }

        public Calendar(Common common)
        {
            this.common = common;
        }

        public Calendar(Common common, Uri Url, NetworkCredential credentials)
        {
            this.Url = Url;
            this.common = common;
            this.Credentials = credentials;
            Initialize();
        }
        

        public void Initialize() {
			var result = common.Request(Url, "PROPFIND", 
                new XDocument(
                    new XElement(CalDav.Common.xDav.GetName("propfind"),
                        //new XElement(CalDav.Common.xDav.GetName("allprop")//,
                        //    //new XElement(xcollectionset)
                        //    )
                        new XElement(CalDav.Common.xDav.GetName("prop"),
                            new XElement(CalDav.Common.xDav.GetName("resourcetype")),
                            new XElement(CalDav.Common.xDav.GetName("displayname")),
                            // new XElement(CalDav.Common.xCalendarServer.GetName("getctag")),
                            new XElement(CalDav.Common.xCalDav.GetName("calendar-description"))
                            )
                        )
                    ),
                    Credentials, new Dictionary<string, string> {
					{ "Depth", "0" }
				});
			var xdoc = XDocument.Parse(result.ResponseContent);
			var desc = xdoc.Descendants(CalDav.Common.xCalDav.GetName("calendar-description")).FirstOrDefault();
			var name = xdoc.Descendants(CalDav.Common.xDav.GetName("displayname")).FirstOrDefault();
			if (name != null) Name = name.Value;
			if (desc != null) Description = desc.Value;

			var resourceTypes = xdoc.Descendants(CalDav.Common.xDav.GetName("resourcetype"));
			if (!resourceTypes.Any(
				r => r.Elements(CalDav.Common.xDav.GetName("collection")).Any()
					&& r.Elements(CalDav.Common.xCalDav.GetName("calendar")).Any()
				))
				throw new Exception("This server does not appear to support CALDAV");
		}

		public CalendarCollection Search(CalDav.CalendarQuery query) {
			var result = common.Request(Url, "REPORT", (XElement)query, Credentials, new Dictionary<string, string> {
				{ "Depth", "1" }
			});
			var xdoc = XDocument.Parse(result.ResponseContent);
			var data = xdoc.Descendants(CalDav.Common.xCalDav.GetName("calendar-data"));
			var serializer = new Serializer();
			return new CalendarCollection(data.SelectMany(x => {
				using (var rdr = new System.IO.StringReader(x.Value)) {
					return serializer.Deserialize<CalendarCollection>(rdr);
				}
			}));
		}

        public string GetSyncToken()
        {
            var requestContent = new XDocument(
                    new XElement(CalDav.Common.xDav.GetName("propfind"),
                        //new XElement(CalDav.Common.xDav.GetName("allprop")//,
                        //    //new XElement(xcollectionset)
                        //    )
                        new XElement(CalDav.Common.xDav.GetName("prop"),
                            new XElement(CalDav.Common.xDav.GetName("displayname")),
                            // new XElement(CalDav.Common.xCalendarServer.GetName("getctag")),
                            new XElement(CalDav.Common.xDav.GetName("sync-token"))
                            )
                        )
                    );

            var result = common.Request(Url, "PROPFIND", requestContent, Credentials, new Dictionary<string, string> {
                { "Depth", "0" }
            });

            var xdoc = XDocument.Parse(result.ResponseContent);
            var desc = xdoc.Descendants(CalDav.Common.xDav.GetName("sync-token")).FirstOrDefault();
            if (desc == null)
            {
                throw new Exception("Server does not support sync-token");
            }

            return desc.Value;
        }

        public string GetSyncChanges(string syncToken)
        {
            var requestContent = new XDocument(
                    new XElement(CalDav.Common.xDav.GetName("sync-collection"),
                        new XElement(CalDav.Common.xDav.GetName("sync-token"), syncToken),
                        new XElement(CalDav.Common.xDav.GetName("sync-level"), "1"),
                        //new XElement(CalDav.Common.xDav.GetName("allprop")//,
                        //    //new XElement(xcollectionset)
                        //    )
                        new XElement(CalDav.Common.xDav.GetName("prop"),
                            new XElement(CalDav.Common.xDav.GetName("getetag"))
                            )
                        )
                    );

            var result = common.Request(Url, "REPORT", requestContent, Credentials, new Dictionary<string, string> {
                { "Depth", "1" }
            });


            //
            // TODO afvangen 403 als er iets met synctoken aan de hand is, zodat we opnieuw kunnen proberen
            //

            throw new NotImplementedException("TODO UPDATED EN DELETED TERUGGEVEN");
            //var xdoc = XDocument.Parse(result.ResponseContent);
            //var desc = xdoc.Descendants(CalDav.Common.xDav.GetName("sync-token")).FirstOrDefault();
            //if (desc == null)
            //{
            //    throw new Exception("Server does not support sync-token");
            //}

            //return desc.Value;
        }

        public void Save(Event e) {
            bool update = !string.IsNullOrEmpty(e.UID);
        
            if (string.IsNullOrEmpty(e.UID)) e.UID = Guid.NewGuid().ToString();
			e.LastModified = DateTime.UtcNow;

            var headers = new Dictionary<string, string>();
            if (!update)
            {
                headers["If-None-Match"] = "*";
            }

            var calendar = new CalDav.Calendar();
            e.Sequence = (e.Sequence ?? 0) + 1;
            calendar.Events.Add(e);
            string content;
            using (var ms = new MemoryStream())
            {
                Common.Serialize(ms, calendar);
                var arr = ms.ToArray();
                content = System.Text.Encoding.UTF8.GetString(arr, 0, arr.Length);
            }


            var result = common.Request(new Uri(Url, e.UID + ".ics"), "PUT", "text/calendar", content, Credentials, headers);
                // (req, str) => {
                //if (!update)
                //{
                    //req.Headers[System.Net.HttpRequestHeader.IfNoneMatch] = "*";
                //}
                //req.ContentType = "text/calendar";
			//	var calendar = new CalDav.Calendar();
		//		e.Sequence = (e.Sequence ?? 0) + 1;
	//			calendar.Events.Add(e);
//				Common.Serialize(str, calendar);

			//}, Credentials);

			if (result.HttpStatusCode != System.Net.HttpStatusCode.Created && result.HttpStatusCode != HttpStatusCode.NoContent)
				throw new Exception("Unable to save event: " + result.HttpStatusCode);
            //e.Url = new Uri(Url, result.Item3[System.Net.HttpRequestHeader.Location]);
            e.Url = new Uri(Url, result.ResponseHeaders["Location"]);

            GetObject(e.UID);
		}
        public void Save(ToDo e)
        {
            bool update = !string.IsNullOrEmpty(e.UID);
           
            if (string.IsNullOrEmpty(e.UID)) e.UID = Guid.NewGuid().ToString();
            e.LastModified = DateTime.UtcNow;

            //                if (!update)
            //{
            //  req.Headers[System.Net.HttpRequestHeader.IfNoneMatch] = "*";
            //}
            //req.ContentType = "text/calendar";
            //var calendar = new CalDav.Calendar();
            //e.Sequence = (e.Sequence ?? 0) + 1;
            //calendar.ToDos.Add(e);
            //Common.Serialize(str, calendar);
            //
            //          }, Credentials);

            var headers = new Dictionary<string, string>();
            if (!update)
            {
                headers["If-None-Match"] = "*";
            }

            var calendar = new CalDav.Calendar();
            e.Sequence = (e.Sequence ?? 0) + 1;
            calendar.ToDos.Add(e);
            string content;
            using (var ms = new MemoryStream())
            {
                Common.Serialize(ms, calendar);
                var arr = ms.ToArray();
                content = System.Text.Encoding.UTF8.GetString(arr, 0, arr.Length);
            }

            var result = common.Request(new Uri(Url, e.UID + ".ics"), "PUT", "text/calendar", content, Credentials, headers);
                //(req, str) => {
                //                if (!update)
                //{
                //  req.Headers[System.Net.HttpRequestHeader.IfNoneMatch] = "*";
                //}
                //req.ContentType = "text/calendar";
                //Common.Serialize(str, calendar);
                //
                //          }, Credentials);

            if (result.HttpStatusCode != System.Net.HttpStatusCode.Created && result.HttpStatusCode != HttpStatusCode.NoContent)
                throw new Exception("Unable to save event: " + result.HttpStatusCode);
            // e.Url = new Uri(Url, result.Item3[System.Net.HttpResponseHeader.Location]);

            GetObject(e.UID);
        }
        public CalendarCollection GetAll() {
			var result = common.Request(Url, "REPORT", CalDav.Common.xCalDav.Element("calendar-multiget",
			CalDav.Common.xDav.Element("prop",
				CalDav.Common.xDav.Element("getetag"),
				CalDav.Common.xCalDav.Element("calendar-data")
				)
			), Credentials, new Dictionary<string, string> { { "Depth", "1" } });




			return null;
		}

		public CalendarCollection GetObject(string uid) {
			var result = common.Request(Url, "REPORT", CalDav.Common.xCalDav.Element("calendar-multiget",
				CalDav.Common.xDav.Element("prop",
					CalDav.Common.xDav.Element("getetag"),
					CalDav.Common.xCalDav.Element("calendar-data")
					),
				CalDav.Common.xDav.Element("href", new Uri(Url, uid + ".ics"))
				), Credentials, new Dictionary<string, string> { { "Depth", "1" } });


			return null;

		}

        public void Save(IEvent e)
        {
            Event ev = (Event)e;
            Save(ev);
        }

        public void Save(IToDo t)
        {
            ToDo todo = (ToDo)t;
            Save(todo);
        }

        public IToDo createToDo()
        {
            ToDo todo = new ToDo();
            return todo;
        }

        public ITrigger createTrigger()
        {
            return new Trigger();
        }

        public IAlarm createAlarm()
        {
            return new Alarm();
        }

        public IEvent createEvent()
        {
            return new Event();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using CalCli.API;

namespace CalDav {
	public class JournalEntry : ICalendarObject {
		public JournalEntry() {
			DTSTAMP = DateTime.UtcNow;
			Properties = new List<NameValuePairWithParameters>();
		}
		DateTime? DTSTAMP;

		public virtual Classes? Class { get; set; }
		public virtual string UID { get; set; }
		public virtual Contact Organizer { get; set; }
		public virtual Statuses? Status { get; set; }
		public virtual ICollection<string> Categories { get; set; }
		public virtual string Description { get; set; }
		public virtual int? Sequence { get; set; }
		public virtual DateTime? LastModified { get; set; }
		public virtual Calendar Calendar { get; set; }
		public ICollection<NameValuePairWithParameters> Properties { get; set; }

		public void Deserialize(System.IO.TextReader rdr, Serializer serializer) {
			string name, value;
			var parameters = new XNameValueCollection();
			while (rdr.Property(out name, out value, parameters) && !string.IsNullOrEmpty(name)) {
				switch (name.ToUpper()) {
					case "CLASS": Class = value.ToEnum<Classes>(); break;
					case "STATUS": Status = value.ToEnum<Statuses>(); break;
					case "UID": UID = value; break;
					case "ORGANIZER":
						Organizer = new Contact();
						Organizer.Deserialize(value, parameters);
						break;
					case "CATEGORIES":
						Categories = value.SplitEscaped().ToList();
						break;
					case "DESCRIPTION": Description = value; break;
					case "SEQUENCE": Sequence = value.ToInt(); break;
					case "LAST-MODIFIED": LastModified = value.ToDateTime(); break;
					case "DTSTAMP": DTSTAMP = value.ToDateTime(); break;
					case "END": return;
					default:
						Properties.Add(new NameValuePairWithParameters(name, value, parameters));
						break;
				}
			}
		}

		public void Serialize(System.IO.TextWriter wrtr) {
			wrtr.BeginBlock("VJOURNAL");
			wrtr.Property("ORGANIZER", Organizer);
			wrtr.Property("CLASS", Class);
			wrtr.Property("UID", UID);
			wrtr.Property("SEQUENCE", Sequence);
			wrtr.Property("LAST-MODIFIED", LastModified);
			wrtr.EndBlock("VJOURNAL");
		}

	}
}

using CalCli.API;
using CalDav;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Specialized;
using System.Net.Mail;

namespace Tests {
	[TestClass]
	public class ParsingBasic {
		[TestMethod]
		public void KeyValue() {
			var values = DeserializeProperty("TEST;VALUE1=ONE;VALUE2=TWO:tested\n\t tested");
			values.Name.ShouldBe("TEST");
			values.Value.ShouldBe("tested tested");
			values.Parameters["VALUE1"].ShouldBe("ONE");
			values.Parameters["VALUE2"].ShouldBe("TWO");
		}

		private static NameValuePairWithParameters DeserializeProperty(string text) {
			using (var rdr = new System.IO.StringReader(text)) {
				string name, value;
				var parameters = new XNameValueCollection();
				rdr.Property(out name, out value, parameters);
				if (name == null) return null;
				return new NameValuePairWithParameters(name, value, parameters);
			}
		}

		private static T Deserialize<T>(string property) where T : class, IHasParameters, new() {
			var t = new T();
			var values = DeserializeProperty(property);
			t.Deserialize(values.Value, values.Parameters);
			return t;
		}

		[TestMethod]
		public void Contact() {
			var text = "ORGANIZER;CN=JohnSmith;DIR=\"ldap" + "://host.com:6666/o=3DDC Associates,c=3DUS??(cn=3DJohn Smith)\":MAILTO" + ":jsmith@host1.com";
			var contact = Deserialize<Contact>(text);

			contact.Name.ShouldBe("JohnSmith");
			contact.Email.ShouldBe("jsmith@host1.com");
			var addr = (XMailAddress)contact;
			addr.DisplayName.ShouldBe("JohnSmith");
			addr.Address.ShouldBe("jsmith@host1.com");

			contact.Directory.ShouldBe("ldap" + "://host.com:6666/o=DC Associates,c=US??(cn=John Smith)");

			var text2 = Serialize("ORGANIZER", contact);
			text2.ShouldBe(text);
		}

		private static string Serialize(string name, IHasParameters obj) {
			return name + Common.FormatParameters(obj.GetParameters()) + ":" + obj.ToString();
		}

		[TestMethod]
		public void Trigger() {
			var text = "TRIGGER;VALUE=DATE-TIME:20120328T133700Z";
			var trigger = Deserialize<Trigger>(text);
			trigger.DateTime.ShouldBe(new DateTime(2012, 3, 28, 13, 37, 0, DateTimeKind.Utc));
			var text2 = Serialize("TRIGGER", trigger);
			text2.ShouldBe(text);

			text = "TRIGGER;RELATED=END:-P1W3DT2H3M45S";
			trigger = Deserialize<Trigger>(text);
			trigger.Related.ShouldBe(Relateds.End);
			trigger.Duration.ShouldBe(-(new TimeSpan(1 * 7 + 3, 2, 3, 45, 0)));
			text2 = Serialize("TRIGGER", trigger);
			text2.ShouldBe(text);
		}
	}
}

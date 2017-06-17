using System.Collections.Specialized;

namespace CalDav {
	public class Contact : IHasParameters {
		public Contact() { }
		public Contact(XMailAddress addr) {
			Name = addr.DisplayName;
			Email = addr.Address;
		}

		public string Name { get; set; }
		public string Email { get; set; }
		public string SentBy { get; set; }
		public string Directory { get; set; }

		public static implicit operator XMailAddress(Contact c) {
			return new XMailAddress(c.Email, c.Name);
		}

		public XNameValueCollection GetParameters() {
			var values = new XNameValueCollection();
			if (!string.IsNullOrEmpty(Name)) values["CN"] = Name;
			if (!string.IsNullOrEmpty(Directory)) values["DIR"] = Directory;
			if (!string.IsNullOrEmpty(SentBy)) values["SENT-BY"] = SentBy;
			return values;
		}

		public override string ToString() {
			return "MAILTO:" + Email;
		}

		public void Deserialize(string value, XNameValueCollection parameters) {
			Email = value.Substring(value.IndexOf(':') + 1);
			Name = parameters["CN"];
			SentBy = parameters["SENT-BY"];
			if (!string.IsNullOrEmpty(SentBy)) {
				SentBy = SentBy.Substring(SentBy.IndexOf(':') + 1);
			}
			Directory = parameters["DIR"];
		}
	}
}

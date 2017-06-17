using System.Collections.Specialized;

namespace CalDav {
	public interface IHasParameters {
		XNameValueCollection GetParameters();
		void Deserialize(string value, XNameValueCollection parameters);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalDav
{
    public class NameValuePairWithParameters
    {
        public NameValuePairWithParameters(string name, string val, XNameValueCollection parameters)
        {
            Name = name;
            Value = val;
            Parameters = parameters;
        }

        public string Name;
        public string Value;
        public XNameValueCollection Parameters;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalDav
{
    public class XMailAddress
    {
        public string Address { get; private set; }
        public string DisplayName { get; private set; }

        public XMailAddress(string address, string displayName)
        {
            Address = address;
            DisplayName = displayName;
        }
    }
}

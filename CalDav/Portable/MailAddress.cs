using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Net.Mail
{
    public class MailAddress
    {
        public string Address { get; private set; }
        public string DisplayName { get; private set; }

        public MailAddress(string address, string displayName)
        {
            Address = address;
            DisplayName = displayName;
        }
    }
}

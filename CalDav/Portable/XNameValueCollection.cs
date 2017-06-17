using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Specialized
{
    public class XNameValueCollection //: Dictionary<string, string>
    {
        private Dictionary<string, string> dictionary = new Dictionary<string, string>();

        public int Count
        {
            get
            {
                return dictionary.Keys.Count;
            }
        }
        
        public string this[string key]
        {
            get
            {
                if (dictionary.ContainsKey(key))
                {
                    return dictionary[key];
                }

                return null;
            }
            set
            {
                dictionary[key] = value;
            }
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public string[] AllKeys
        {
            get
            {
                return dictionary.Keys.ToArray();
            }
        }
    }
}

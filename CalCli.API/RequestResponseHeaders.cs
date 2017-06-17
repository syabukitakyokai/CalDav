using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalCli.API
{
    public class RequestResponseHeaders : IEnumerable<KeyValuePair<string, string>> //: Dictionary<string, string>
    {
        private Dictionary<string, string> dictionary = new Dictionary<string, string>();

        //public int Count
        //{
        //    get
        //    {
        //        return dictionary.Keys.Count;
        //    }
        //}

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

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalDav
{
    public class XSqlDateTime
    {
        public static readonly DateTime? MinValue = new DateTime(1753, 1, 1, 0, 0, 0);
        public static readonly DateTime? MaxValue = new DateTime(9999, 12, 31, 23, 59, 59);
    }
}

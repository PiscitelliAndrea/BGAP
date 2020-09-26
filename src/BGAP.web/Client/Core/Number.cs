using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BGAP.web.Client.Core
{
    public class Number
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public int number { get; set; }
        public string backgroundColor { get; set; }

        public string NumberValue
        {
            get
            {
                return number != 0 ? number.ToString() : "";
            }
        }
    }
}

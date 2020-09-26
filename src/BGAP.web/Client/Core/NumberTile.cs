using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BGAP.web.Client.Core
{
    public class NumberTile
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public int Number { get; set; }
        public string BackgroundColor { get; set; }

        public string NumberValue
        {
            get
            {
                return Number != 0 ? Number.ToString() : "";
            }
        }
    }
}

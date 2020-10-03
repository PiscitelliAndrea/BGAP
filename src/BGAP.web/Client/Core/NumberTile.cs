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
        private int Number { get; set; }
        public string BackgroundColor { get; set; }

        public void ClearNumber()
        {
            this.Number = 0;
        }

        public void SetNumber(int value)
        {
            this.Number = value;
        }

        public void AddNumber(int value)
        {
            this.Number += value;
        }

        public string NumberValue
        {
            get
            {
                return Number != 0 ? Number.ToString() : "";
            }
        }
    }
}

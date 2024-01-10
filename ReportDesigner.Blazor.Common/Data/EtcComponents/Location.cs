using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportDesigner.Blazor.Common.Data.EtcComponents
{
    public class Location
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Location()
        {

        }
        public Location(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}

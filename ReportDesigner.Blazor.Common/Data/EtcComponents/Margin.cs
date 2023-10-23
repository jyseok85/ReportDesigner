using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportDesigner.Blazor.Common.Data.EtcComponents
{    
    public class Margin
    {
        public int Left { get; set; } = 10;
        public int Top { get; set; } = 10;
        public int Right { get; set; } = 10;
        public int Bottom { get; set; } = 10;

        public Margin()
        {
        }
        public Margin(int top, int right, int bottom, int left) {
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;   
            this.Left = left;
        } 
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportDesigner.Blazor.Common.Data.EtcComponents
{
    public class Border
    {
        public int Left { get; set; } = 1;
        public string LeftColor { get; set; } = "transparent";
        public int Top { get; set; } = 1;
        public string TopColor { get; set; } = "transparent";
        public int Right { get; set; } = 1;
        public string RightColor { get; set; } = "transparent";
        public int Bottom { get; set; } = 1;
        public string BottomColor { get; set; } = "transparent";

        public Border()
        {
        }
        public Border(int top, int right, int bottom, int left)
        {
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
            this.Left = left;
        }
    }
}

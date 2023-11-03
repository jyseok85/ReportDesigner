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
        //2023.11.03
        //개별 테두리 두께 및 컬러는 적용할지 말지 판단 못함. 일단 속성은 만들어둠
        //일단 개별로 테두리 표시 만 적용

        public bool UseIndividualBorders = false;
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

        public void SetBorderThickness(int thickness)
        {
            this.Top = thickness;
            this.Right = thickness;
            this.Bottom = thickness;
            this.Left = thickness;
        }

        public void SetAllBorderColor(string color)
        {
            this.TopColor = color;
            this.RightColor = color;
            this.LeftColor = color;
            this.BottomColor = color;
        }
    }
}

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

        public bool Use { get; set; } = true;
        public string Style { get; set; } = "solid";

        private string color = "Black";
        public string Color { 
            get { return color; } 
            set
            {
                SetAllBorderColor(value);
                color = value;
            }
        }

        public int Thickness { 
            get { return Left; }
            set { SetBorderThickness(value); }
        }
        public bool UseIndividualBorders = false;
        public int Left { get; set; } = 1;
        public int Top { get; set; } = 1;
        public int Right { get; set; } = 1;
        public int Bottom { get; set; } = 1;
        public bool UseLeftBorder { get; set; } = true;
        public bool UseTopBorder { get; set; } = true;
        public bool UseRightBorder { get; set; } = true;
        public bool UseBottomBorder { get; set; } = true;
        public string TopColor { get; set; } = "transparent";
        public string LeftColor { get; set; } = "transparent";
        public string RightColor { get; set; } = "transparent";
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
            if(UseTopBorder)
                this.TopColor = color;
            else
                this.TopColor = "transparent";
            if(UseRightBorder)
                this.RightColor = color;
            else
                this.RightColor = "transparent";

            if(UseBottomBorder) 
                this.BottomColor = color;
            else
                this.BottomColor = "transparent";

            if(UseLeftBorder) 
                this.LeftColor = color;
            else
                this.LeftColor = "transparent";

        }

        public void UpdateBorderColor()
        {
            
            if (UseTopBorder)
                this.TopColor = this.color;
            else
                this.TopColor = "transparent";
            if (UseRightBorder)
                this.RightColor = this.color; 
            else
                this.RightColor = "transparent";

            if (UseBottomBorder)
                this.BottomColor = this.color; 
            else
                this.BottomColor = "transparent";

            if (UseLeftBorder)
                this.LeftColor = this.color; 
            else
                this.LeftColor = "transparent";
        }
    }
}

using Microsoft.AspNetCore.Components.Web.Virtualization;
using ReportDesigner.Blazor.Common.Data.EtcComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportDesigner.Blazor.Common.Data.Model
{
    public class ReportComponentModel
    {
        public string Uid { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Height { get; set; } = 0;
        public int Width { get; set; } = 0;
        public virtual int X { get; set; } = 0;
        public virtual int Y { get; set; } = 0;
        public virtual int Right { get { return X + Width; } set { } }
        public virtual int Bottom { get { return Y + Height; } set { } }
        public bool DrawBorder { get; set; } = true;
        private bool selected = false;
        public bool Selected
        {
            get { return this.selected; }
            set
            {
                this.selected = value;
                if (value == true)
                {
                }
                else
                {
                }
            }
        }



        public bool Hidden { get; set; } = false;
        public Margin Margin { get; set; } = new Margin();
        public Border Border { get; set; } = new Border();
    }
}

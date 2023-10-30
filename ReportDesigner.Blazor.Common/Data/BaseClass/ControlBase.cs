using Microsoft.AspNetCore.Components;
using ReportDesigner.Blazor.Common.Data.Model;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;

namespace ReportDesigner.Blazor.Common.Data.BaseClass
{
    public class ControlBase : ComponentBase
    {
        public int TabIndex { get; set; } = 0;

        public ControlModel Model { get; set; } = new ControlModel();


        public ControlBase() {
        }

        public ControlBase(int x, int y, int width, int height, int tabIndex)
        {
            Model.X = x;
            Model.Y = y;
            Model.Width = width;
            Model.Height = height;
            TabIndex = tabIndex;
        }
    }
}

using Microsoft.AspNetCore.Components;
using ReportDesigner.Blazor.Common.Data.Model;

namespace ReportDesigner.Blazor.Common.Data.BaseClass
{
    public class LayerBase : ComponentBase
    {
        public int ZIndex { get; set; } = 0;

        public LayerModel Model { get; set; } = new LayerModel();

        public LayerBase()
        {
            this.Model.Type = ReportComponentModel.Control.Layer;

        }
    }
}

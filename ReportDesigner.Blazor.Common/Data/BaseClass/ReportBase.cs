using Microsoft.AspNetCore.Components;
using ReportDesigner.Blazor.Common.Data.Model;

namespace ReportDesigner.Blazor.Common.Data.BaseClass
{
    public class ReportBase : ComponentBase
    {
        public ReportModel Model { get; set; } = new ReportModel();
        public string GetPaddingForCss()
        {
            return $" padding:{Model.Margin.Top}px {Model.Margin.Right}px {Model.Margin.Bottom}px {Model.Margin.Left}px; ";
        }
    }
}

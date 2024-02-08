using Microsoft.AspNetCore.Components;
using ReportDesigner.Blazor.Common.Services;
using ReportDesigner.Blazor.Common.Utils;

namespace ReportDesigner.Blazor.Common.UI.Layout.RightSideBarItems
{
    public class PropertyBase : ComponentBase
    {
        [Inject]
        DesignerOptionService Options { get; set; }

        internal void RefreshState()
        {
            this.StateHasChanged();
        }

        internal void OnExpandedChange(string target, bool value)
        {
            Options.SetPanelMenuState(target,value);
            Logger.Instance.Write(this.GetType().Name);
        }
    }
}

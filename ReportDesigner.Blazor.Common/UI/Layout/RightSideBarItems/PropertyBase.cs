using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportDesigner.Blazor.Common.UI.Layout.RightSideBarItems
{
    public class PropertyBase : ComponentBase
    {        
        public void RefreshState()
        {
            this.StateHasChanged();
        }
    }
}

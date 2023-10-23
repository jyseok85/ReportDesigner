using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportDesigner.Blazor.Common.Data.Model
{
    public class ControlModel : ReportComponentModel
    {
        public int TabIndex { get; set; } = 0;

        public ControlModel() { }

    }
}

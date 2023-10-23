using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportDesigner.Blazor.Common.Data.Model
{
    public class LayerModel : ReportComponentModel
    {
        public int ZIndex { get; set; } = 0;

        public LayerModel()
        {

        }
    }
}

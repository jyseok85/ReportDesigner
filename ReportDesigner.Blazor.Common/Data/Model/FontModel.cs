using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportDesigner.Blazor.Common.Data.Model
{
    public class FontModel
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public FontModel(string name)
        {
            this.Name = name;
        }
    }
}

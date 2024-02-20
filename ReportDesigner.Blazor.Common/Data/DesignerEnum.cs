using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportDesigner.Blazor.Common.Data
{
    public enum TextDirection
    {
        Horizontal,
        Vertical,
        RotateAllText90,
        RotateAllText270,
        /// <summary>
        /// UI의 마지막 항목, Word랑 PPT랑 좀 다르게 동작함.
        /// </summary>
        Stacked
    }

    public enum OverFlowText
    {
        Visible,
        Clip,
        Ellipsis,
        WordWrap
    }
}

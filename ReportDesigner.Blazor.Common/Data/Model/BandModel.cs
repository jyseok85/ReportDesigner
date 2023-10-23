using Microsoft.AspNetCore.Components;
using ReportDesigner.Blazor.Common.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportDesigner.Blazor.Common.Data.Model
{
    public class BandModel : ReportComponentModel
    {
        [DefaultValue(Content)]
        public enum Band
        {
            ReportHeader,
            PageHeader,
            Content,
            DataHeader,
            Data,
            DataFooter,
            PageFooter,
            ReportFooter
        }
        public Band Type { get; set; } = Band.Content;
    }
}

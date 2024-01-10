using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportDesigner.Blazor.Common.Data.EtcComponents
{
    public class Font : ICloneable
    {
        public string FontFamily { get; set; } = "Noto Sans";
        public int FontSize { get; set; } = 14;

        public string FontColor { get; set; } = "Black";

        /// <summary>
        /// font-style : italic
        /// font-weight: bold;
        /// text-decoration : underline
        /// text-decoration : line-through
        /// </summary>
        public string FontStyle { get; set; } = string.Empty;

        public string VerticlaAlignment { get; set; } = "center";

        public string HorizontalAlignment { get; set; } = "center";

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}

using Microsoft.Extensions.Options;
using ReportDesigner.Blazor.Common.Data.EtcComponents;
using System.Drawing;

namespace ReportDesigner.Blazor.Common.Services
{
    public class DesignerOptionService
    {
        public Margin PaperMargin { get; set; } = new Margin(38,38,38,38);

        public bool IsLandscape { get; set; } = false;

        private Size paperSize = new Size(798, 1124);

        public Size PaperSize { 
            get { return this.paperSize; }
            set { this.paperSize = value;
                FirePaperSizeChangedEvent();
            }
        }

        public void FirePaperSizeChangedEvent(int delay = 0)
        {            
            PaperSizeChanged?.Invoke(paperSize.Width, delay);
        }
        public event EventHandler<int>? PaperSizeChanged;

    }

    

}

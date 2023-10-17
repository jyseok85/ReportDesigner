using Microsoft.Extensions.Options;
using System.Drawing;

namespace ReportDesigner.Blazor.Common.Services
{
    public class DesignerOptionService
    {
        public Margin PaperMargin { get; set; } = new Margin();

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

    public class Margin
    {
        public int Left { get; set; } = 10;
        public int Top { get; set; } = 10;
        public int Right { get; set; } = 10;
        public int Bottom { get; set; } = 10;
    }

}

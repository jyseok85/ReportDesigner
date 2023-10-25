using Microsoft.Extensions.Options;
using ReportDesigner.Blazor.Common.Data.EtcComponents;
using ReportDesigner.Blazor.Common.Data.Model;
using ReportDesigner.Blazor.Common.UI.ReportControls;
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

        public enum ActionState
        {
            Create,
            Resize,
            None
        }
        public ActionState State { get; set; } = ActionState.None;

        private Dictionary<string, ReportComponentModel> controlDictionary = new Dictionary<string, ReportComponentModel>();

        public Dictionary<string, ReportComponentModel> ControlDictionary { get { return controlDictionary; } }

        public void AddControl(string key, ReportComponentModel model)
        {
            Console.WriteLine(key);

            if (model.Type != ReportComponentModel.Control.Report)
            {
                model.AbsoluteOffsetX = model.X + PaperMargin.Left;
                model.AbsoluteOffsetY = model.X + PaperMargin.Top;
                model.AbsoluteOffsetRight = model.X + model.Width;
                model.AbsoluteOffsetBottom = model.Y + model.Height;
            }

            controlDictionary.Add(key, model);
        }

        public void UpdateAllControlOffset()
        {
            foreach (ReportComponentModel model in controlDictionary.Values)
            {
                if(model.Type == ReportComponentModel.Control.Report)
                {
                    model.Width = PaperSize.Width;
                    model.Height = PaperSize.Height;
                    model.Margin = PaperMargin;
                }

                if(model.Type != ReportComponentModel.Control.Report)
                {
                    model.AbsoluteOffsetX = model.X + PaperMargin.Left;
                    model.AbsoluteOffsetY = model.X + PaperMargin.Top;
                    model.AbsoluteOffsetRight = model.X + model.Width;
                    model.AbsoluteOffsetBottom = model.Y + model.Height;
                }

                if(model.Type == ReportComponentModel.Control.Band)
                {
                    model.Width = paperSize.Width - PaperMargin.Left - PaperMargin.Right;
                }
            }
        }

    }



}

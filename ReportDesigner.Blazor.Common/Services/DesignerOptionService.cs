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

        public bool UseSnap { get; set; } = true;
        public int SnapPoint { get; set; } = 10;
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
            None,
            Drag
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
                model.AbsoluteOffsetRight = model.AbsoluteOffsetX + model.Width;
                model.AbsoluteOffsetBottom = model.AbsoluteOffsetY + model.Height;
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
                    model.AbsoluteOffsetY = model.Y + PaperMargin.Top;
                    model.AbsoluteOffsetRight = model.AbsoluteOffsetX + model.Width;
                    model.AbsoluteOffsetBottom = model.AbsoluteOffsetY + model.Height;
                }

                if(model.Type == ReportComponentModel.Control.Band)
                {
                    model.Width = paperSize.Width - PaperMargin.Left - PaperMargin.Right;
                }
            }
        }



        public SnapLinerModel SnapLinerModel => this.snapLinerModel;
        private SnapLinerModel snapLinerModel = new SnapLinerModel();


        List<int> snapAbsoluteX = new List<int>();
        List<int> snapAbsoluteY = new List<int>();
        /// <summary>
        /// 현재 밴드에 포함된 모든 컨트롤의 스냅 위치를 계산한다.
        /// 1. 컨트롤 생성, 삭제
        /// 2. 컨트롤 이동
        /// 3. 컨트롤 사이즈 변경
        /// </summary>
        public void UpdateSnapControl()
        {
            snapAbsoluteX.Clear();
            snapAbsoluteY.Clear();
            
            foreach (ReportComponentModel control in controlDictionary.Values)
            {

                int x = (int)control.AbsoluteOffsetX;
                int y = (int)control.AbsoluteOffsetY;
                int right = (int)control.AbsoluteOffsetRight;
                int bottom = (int)control.AbsoluteOffsetBottom;
                if (snapAbsoluteX.Contains(x) == false)
                    snapAbsoluteX.Add(x);
                if (snapAbsoluteY.Contains(bottom) == false)
                    snapAbsoluteY.Add(bottom);
                if (snapAbsoluteY.Contains(y) == false)
                    snapAbsoluteY.Add(y);
                if (snapAbsoluteX.Contains(right) == false)
                    snapAbsoluteX.Add(right);
            }          
        }

        public void HideSnap()
        {
            snapLinerModel.HideSnapLine();
        }
        public Point DoSnap(Dictionary<string, int> dragObjectSnapPoint, int width, int height)
        {
            Point snapPoint = new Point(-999, -999);

            if (this.UseSnap)
            {
                foreach (KeyValuePair<string, int> point in dragObjectSnapPoint)
                {
                    int findValue = point.Value;
                    int value = 0;

                    if (point.Key.Equals("left") || point.Key.Equals("right"))
                        value = snapAbsoluteX.OrderBy(x => Math.Abs(findValue - x)).First();
                    else
                        value = snapAbsoluteY.OrderBy(x => Math.Abs(findValue - x)).First();

                    if (Math.Abs(findValue - value) < 10)
                    {
                        snapLinerModel.ShowSnapLine(point.Key, true, value);
                        if (Math.Abs(findValue - value) < this.SnapPoint)
                        {
                            if (point.Key == "left")
                                snapPoint.X = value;
                            if (point.Key == "top")
                                snapPoint.Y = value;
                            if (point.Key == "right")
                                snapPoint.X = value - width;
                            if (point.Key == "bottom")
                                snapPoint.Y = value - height;
                        }
                    }
                    else
                    {
                        snapLinerModel.ShowSnapLine(point.Key, false, value);
                    }
                }
            }

            return snapPoint;
        }
    }



}

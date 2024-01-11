using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using ReportDesigner.Blazor.Common.Data.EtcComponents;
using ReportDesigner.Blazor.Common.Data.Model;
using ReportDesigner.Blazor.Common.UI.ReportControls;
using System.Drawing;
using System.Reflection;

namespace ReportDesigner.Blazor.Common.Services
{
    public class DesignerOptionService
    {
        [Inject]        
        public required SelectedControlService SelectedControl { get; set; }

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
        
        public event EventHandler<string>? Refresh;

        public event EventHandler<string>? ControlSelectionChanged;

        public void RefreshBody()
        {
            Refresh?.Invoke(null, "body");
        }

        public void RefreshRightPanel()
        {
            Refresh?.Invoke(null, "Right");
        }

        public void FireControlSelectionChangedEvent()
        {
            ControlSelectionChanged?.Invoke(null, "Right");
        }

        public enum ActionState
        {
            Create,
            Resize,
            None,
            Edit,
            Drag
        }
        public ActionState State { get; set; } = ActionState.None;

        private Dictionary<string, ReportComponentModel> controlDictionary = new Dictionary<string, ReportComponentModel>();

        public Dictionary<string, ReportComponentModel> ControlDictionary { get { return controlDictionary; } }

        public List<ReportComponentModel> ComponentList = new List<ReportComponentModel>();
        public void AddControl(string uid, ReportComponentModel model)
        {
            Console.WriteLine(uid);

            if (model.Type != ReportComponentModel.Control.Report)
            {
                model.AbsoluteOffsetX = model.X + PaperMargin.Left;
                model.AbsoluteOffsetY = model.X + PaperMargin.Top;
                model.AbsoluteOffsetRight = model.AbsoluteOffsetX + model.Width;
                if (model.Type == ReportComponentModel.Control.Band)
                    model.AbsoluteOffsetRight = PaperSize.Width - PaperMargin.Right;

                model.AbsoluteOffsetBottom = model.AbsoluteOffsetY + model.Height;

            }
            model.Name = GenerateName(model.Type);  

            controlDictionary.Add(uid, model);
            
            ComponentList.Add(model);

            //컨트롤 트리용으로 별도로 생성한다. ?? 과연 최적화에 있어서 이게 맞는걸까?? 해당 탭(UI)가 표시될때 기존 데이터로 만들어주는게 낫지 않을까?
            //고민하던 중.. 역시 괜히 해당 모델 만들어서 메모리 올리는것보다 기존 ControlDictionary를 사용해서 UI 표시될때 동적으로 생성해 주는게 더 좋을듯하다.
            //그리고, ControlAddWorkIndex 를 추가로 생성하고, 컨트롤이 추가될때만 이 인덱스스를 변경시키고, 그 값이 변경이 되었을 경우에만 동적으로 갱신을 해주는 것이 좋을 것 같다. 

            string GenerateName(ReportComponentModel.Control type)
            {
                for (int i = 1; i < 1000; i++)
                {
                    string name = type.ToString() + "_" + i.ToString("D3");
                    var data = controlDictionary.Select(x=> x.Value).Where(x => x.Name == name);
                    if(data.Count() == 0)
                    {
                        return name;
                    }
                }

                //todo :컨트롤이 많아질경우 저 Linq의 속도 계산을 해봐야 함.
                throw new Exception("동일한 컨트롤을 1000개 이상 생성할 수 없습니다.");
            }
        }

        public void RemoveControl(string uid)
        {
            var control = controlDictionary[uid];
            this.ComponentList.Remove(control);
            this.controlDictionary.Remove(uid);
        }
        public void UpdateAllControlOffset()
        {
            Console.WriteLine(this.GetType().Name);
            foreach (ReportComponentModel model in controlDictionary.Values)
            {
                if(model.Type == ReportComponentModel.Control.Report)
                {
                    model.Width = PaperSize.Width;
                    model.Height = PaperSize.Height;
                    model.Margin = PaperMargin;
                }

                if(model.Type == ReportComponentModel.Control.Layer)
                {
                    model.AbsoluteOffsetX = model.X + PaperMargin.Left;
                    model.AbsoluteOffsetY = model.Y + PaperMargin.Top;
                    model.AbsoluteOffsetRight = model.AbsoluteOffsetX + model.Width;
                    model.AbsoluteOffsetBottom = model.AbsoluteOffsetY + model.Height;
                }

                if(model.Type == ReportComponentModel.Control.Band)
                {
                    model.AbsoluteOffsetX = model.X + PaperMargin.Left;
                    model.AbsoluteOffsetY = model.Y + PaperMargin.Top;
                    model.Width = paperSize.Width - PaperMargin.Left - PaperMargin.Right;
                    model.AbsoluteOffsetRight = model.AbsoluteOffsetX + model.Width;
                    model.AbsoluteOffsetBottom = model.AbsoluteOffsetY + model.Height;
                }

                if (model.Type == ReportComponentModel.Control.Label || model.Type == ReportComponentModel.Control.None)
                {
                    string uid = model.ParentUid;
                    var bandModel = ControlDictionary[uid];

                    model.AbsoluteOffsetX = model.X + bandModel.AbsoluteOffsetX;
                    model.AbsoluteOffsetY = model.Y + bandModel.AbsoluteOffsetY;
                    model.AbsoluteOffsetRight = model.AbsoluteOffsetX + model.Width;
                    model.AbsoluteOffsetBottom = model.AbsoluteOffsetY + model.Height;
                }

            }

            UpdateSnapControl();
            CheckAreaOverlap();
        }
        /// <summary>
        /// 현재 선택된 컨트롤이 다른 컨트롤과 겹치는 계산해서, 겹칠경우 IsOverlap 속성을 변경합니다.
        /// </summary>
        private void CheckAreaOverlap()
        {
            if (SelectedControl.CurrentSelectedModel is null)
                return;

            int x1 = (int)SelectedControl.CurrentSelectedModel.X;
            int x2 = (int)SelectedControl.CurrentSelectedModel.Right;
            int y1 = (int)SelectedControl.CurrentSelectedModel.Y;
            int y2 = SelectedControl.CurrentSelectedModel.Bottom;

            SelectedControl.CurrentSelectedModel.IsOverlap = false;

            
            //todo : 이거 이중으로 한것좀 처리하자 
            if (IsDragAbleControl(SelectedControl.CurrentSelectedModel.Type))
            {
                foreach (ReportComponentModel model in controlDictionary.Values)
                {
                    if(IsDragAbleControl(model.Type))
                    {
                        if (model.Equals(SelectedControl.CurrentSelectedModel))
                            continue; 
                        int cx1 = (int)model.X;
                        int cx2 = (int)model.Right;
                        int cy1 = (int)model.Y;
                        int cy2 = (int)model.Bottom;

                        if ((cx2 > x1 && cx1 < x2) && (cy2 > y1 && cy1 < y2))
                        {
                            model.IsOverlap = true;
                            SelectedControl.CurrentSelectedModel.IsOverlap = true;
                        }
                        else
                            model.IsOverlap = false;
                    }
                }            

            }

        }

        private bool IsDragAbleControl(ReportComponentModel.Control type)
        {
            switch(type)
            { 
                case ReportComponentModel.Control.Label:
                case ReportComponentModel.Control.None:
                    return true;
                default:
                    return false;
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

        /// <summary>
        /// 모든 컨트롤에 대해서 편집모드 해제
        /// </summary>
        public void TurnOffEditModeForAllControls()
        {
            foreach (ReportComponentModel model in controlDictionary.Values)
            {
                model.IsEditMode = false;
            }
        }

        /// <summary>
        /// 현재 선택된 컨트롤을 키보드 방향의 가장 가까운 스냅포인트로 이동합니다.
        /// </summary>
        /// <param name="key">방향키</param>
        public void SetSnapPoint(string key)
        {
            Console.WriteLine($"{key} {string.Join(",", snapAbsoluteX)}");
            List<int> movement = new List<int>();

            string uid = SelectedControl.CurrentSelectedModel.ParentUid;
            var bandModel = ControlDictionary[uid];

            switch (key)
            {
                case "ArrowRight":
                    {  
                        AddPoint(GetRightSnapPoint(SelectedControl.CurrentSelectedModel.AbsoluteOffsetRight));
                        AddPoint(GetRightSnapPoint(SelectedControl.CurrentSelectedModel.AbsoluteOffsetX));

                        if (movement.Count > 0)
                        {
                            //제일 조금 이동하는 거리를 가져온다.
                            int nextMovement = movement.OrderBy(x => x).First();

                            //밴드영역을 벗어나지 못하도록 한다. 
                            if ((nextMovement + SelectedControl.CurrentSelectedModel.Right) < bandModel.AbsoluteOffsetRight)
                                SelectedControl.CurrentSelectedModel.X += nextMovement;
                            else
                                SelectedControl.CurrentSelectedModel.X = bandModel.Width - SelectedControl.CurrentSelectedModel.Width;
                        }
                    }
                    break;
                case "ArrowLeft":
                    {
                        AddPoint(GetLeftSnapPoint(SelectedControl.CurrentSelectedModel.AbsoluteOffsetRight));
                        AddPoint(GetLeftSnapPoint(SelectedControl.CurrentSelectedModel.AbsoluteOffsetX));

                        if (movement.Count > 0)
                        {
                            //제일 조금 이동하는 거리를 가져온다.
                            int nextMovement = movement.OrderBy(x => x).First();

                            if (SelectedControl.CurrentSelectedModel.X - nextMovement > bandModel.X) 
                                SelectedControl.CurrentSelectedModel.X -= nextMovement;
                            else
                                SelectedControl.CurrentSelectedModel.X = bandModel.X;
                        }
                    }
                    break;
                case "ArrowDown":
                    {
                        AddPoint(GetBottomSnapPoint(SelectedControl.CurrentSelectedModel.AbsoluteOffsetBottom));
                        AddPoint(GetBottomSnapPoint(SelectedControl.CurrentSelectedModel.AbsoluteOffsetY));
                        
                        //오른쪽이나 아래쪽으로 1픽셀씩 더 나간다. 아마도 보더때문인듯한데??
                        if (movement.Count > 0)
                        {
                            //제일 조금 이동하는 거리를 가져온다.
                            int nextMovement = movement.OrderBy(x => x).First();
                            if ((nextMovement + SelectedControl.CurrentSelectedModel.Bottom) < bandModel.AbsoluteOffsetBottom)
                                SelectedControl.CurrentSelectedModel.Y += nextMovement;
                            else
                                SelectedControl.CurrentSelectedModel.Y = bandModel.Height - SelectedControl.CurrentSelectedModel.Height;
                            Console.WriteLine(SelectedControl.CurrentSelectedModel.Y);
                        }
                    }
                    break;
                case "ArrowUp":
                    {
                        AddPoint(GetTopSnapPoint(SelectedControl.CurrentSelectedModel.AbsoluteOffsetBottom));
                        AddPoint(GetTopSnapPoint(SelectedControl.CurrentSelectedModel.AbsoluteOffsetY));

                        if (movement.Count > 0)
                        {
                            //제일 조금 이동하는 거리를 가져온다.
                            int nextMovement = movement.OrderBy(x => x).First();

                            if (SelectedControl.CurrentSelectedModel.Y - nextMovement > bandModel.Y)
                                SelectedControl.CurrentSelectedModel.Y -= nextMovement;
                            else
                                SelectedControl.CurrentSelectedModel.Y = bandModel.Y;
                        }
                    }
                    break;
            }
            void AddPoint(int point)
            {
                if (point > -1)
                {
                    if (movement.Contains(point) == false)
                        movement.Add(point);
                }
            }

            //우측으로 이동할 거리를 가져온다. 
            int GetRightSnapPoint(int currentX)
            {
                var selected1 = snapAbsoluteX.Where(n => n > currentX);
                if (selected1.Count() > 0)
                {
                    var point = selected1.OrderBy(x => x).First();
                    return point - currentX;
                }
                else
                    return -1;
            }

            int GetLeftSnapPoint(int currentX)
            {
                var selected1 = snapAbsoluteX.Where(n => n < currentX);
                if (selected1.Count() > 0)
                {
                    var point = selected1.OrderBy(x => x).Last();
                    return currentX - point;
                }
                else
                    return -1;
            }

            int GetBottomSnapPoint(int currentY)
            {
                var selected1 = snapAbsoluteY.Where(n => n > currentY);
                if (selected1.Count() > 0)
                {
                    var point = selected1.OrderBy(x => x).First();
                    return point - currentY;
                }
                else
                    return -1;               
            }

            int GetTopSnapPoint(int currentY)
            {
                var selected1 = snapAbsoluteY.Where(n => n < currentY);
                if (selected1.Count() > 0)
                {
                    var point = selected1.OrderBy(x => x).Last();
                    return currentY - point;
                }
                else
                    return -1;
            }
        }

        public bool GetMoveableControl()
        {
            switch (SelectedControl.CurrentSelectedModel.Type)
            {
                case ReportComponentModel.Control.Report:
                case ReportComponentModel.Control.Band:
                case ReportComponentModel.Control.Layer:
                    return false;
                case ReportComponentModel.Control.Label:
                    return true;
            }

            return false;
        }

        public Dictionary<string, bool> panelMenuState = new Dictionary<string, bool>();

        public bool GetPanelMenuState(string key)
        {
            if (panelMenuState.ContainsKey(key))
                return panelMenuState[key];
            else
                panelMenuState.Add(key, false);
            return false;
        }

        public void SetPanelMenuState(string key, bool value)
        {
            if (panelMenuState.ContainsKey(key))
                panelMenuState[key] = value;
            else
                panelMenuState.Add(key, false);
        }
        public Dictionary<string, bool> PanelMenuState
        {
            get {

                if (panelMenuState.Count == 0)
                {
                    panelMenuState.Add("PaperSetting", false);
                    panelMenuState.Add("FontPropertyy", false);
                    panelMenuState.Add("Design", false);
                    panelMenuState.Add("Appearance", false);
                    panelMenuState.Add("Control", false);
                    panelMenuState.Add("Layout", false);
                }

                return panelMenuState; }
            set
            {
                if(panelMenuState.Count == 0)
                {
                    panelMenuState.Add("PaperSetting", false);
                    panelMenuState.Add("FontPropertyy", false);
                    panelMenuState.Add("Design", false);
                    panelMenuState.Add("Appearance", false);
                    panelMenuState.Add("Control", false);
                    panelMenuState.Add("Layout", false);
                }

                panelMenuState = value;
            }
        }

        public void SaveBrowserCache(string key, object value)
        {
            //todo : 브라우저 캐시 저장.
        }

        public object LoadBrowserCache(string key)
        {
            return null;
        }
    }
}

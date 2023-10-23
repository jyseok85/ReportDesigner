using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ReportDesigner.Blazor.Common.Data.Model;
using ReportDesigner.Blazor.Common.Services;
using ReportDesigner.Blazor.Common.UI.Report_Controls.Controls;
using System.ComponentModel;

namespace ReportDesigner.Blazor.Common.Data.BaseClass
{
    public class BandBase : ComponentBase
    {
        [Inject]
        ControlCreationService CreationService { get; set; }

        [Inject]
        SelectedControlService Selectedservice { get; set; }

        //public enum ActionState
        //{
        //    Create,
        //    Resize,
        //    None
        //}
        //public ActionState State { get; set; } = ActionState.None;

        public List<ControlBase> controlBases = new List<ControlBase>();

        public BandModel Model { get; set; } = new BandModel();

        //public CreationModel creationModel = new CreationModel();
        public void OnPointerUp(PointerEventArgs e)
        {
            return;
            //if (CreationService.State == ControlCreationService.ActionState.Create)
            //{
            //    var control = controlBases.Find(x => x.Selected == true);
            //    //if (CreationService.State == ActionState.Create)
            //    {
            //        //드래그는 우측 하단으로만 진행하도록 한다.
            //        //if (e.ClientX > CreationService.ClientX && e.ClientY > CreationService.ClientY)
            //        //    CreateControl(e);
            //        CreationService.ActionEnd();
            //    }
            //    //else if (State == ActionState.Resize)
            //    //{

            //    //    if (control != null)
            //    //    {
            //    //        //control.ApplyResize();
            //    //    }
            //    //}
            
            //    if (control is not null)
            //    {
            //        SortControls();
            //        //control.IsDragAble = false;
            //    }

            //}
            //CreationService.State = ControlCreationService.ActionState.None;
            //Console.WriteLine("Band - OnPointerUp");
        }

        public void OnPointerDown(PointerEventArgs e)
        {
            //좌측 컨트롤을 클릭하면 생성모드로 진입한다.
            if (CreationService.State == ControlCreationService.ActionState.Create)
            {
                CreationService.CurrentBand = this;
                //자식 컴포넌트가 선택되지 않았다면 선택된 모든 컴포넌트를 해제해준다.
                DeselectAllControls();

                //2. 생성 모드일 경우(임시로 항상 생성모드로 한다.)

                CreationService.ActionStart(e);
            }
            else
                Selectedservice.OnPointerDown(e, this.Model);

            //text1 = $"Start Point : {(int)e.OffsetX} {(int)e.OffsetY}";
            //text2 = text3 = "";
        }

        public void OnPointerMove(PointerEventArgs e)
        {
             CreationService.ActionMove(e);
        }

        //private bool CreateControl(PointerEventArgs e) //int x, int y, int width, int height)
        //{
        //    int x = CreationService.X;
        //    int y = CreationService.Y;
        //    int width = CreationService.Width;
        //    int height = CreationService.Height;

        //    //최소 사이즈 이상 드래그 된 경우만 진행한다. ?? 아니면 작게 그리면 최소사이즈만큼 그려준다?
        //    int controlMinimumSize = 10;
        //    if (width < controlMinimumSize || height < controlMinimumSize)
        //        return false;

        //    //새로 생성하는 컨트롤에 TabIndex를 할상해서 키보드 이벤트를 받도록 한다. 
        //    int tabIndex = 0;
        //    if (controlBases.Count > 0)
        //        tabIndex = controlBases.Max(x => x.TabIndex) + 1;

        //    var control = new ControlBase()
        //    {
        //        Model.Uid = Guid.NewGuid().ToString(),
        //        Model.X = x,
        //        Y = y,
        //        Width = width,
        //        Height = height,
        //        TabIndex = tabIndex
        //    };

        //    //컴포넌트 목록에 추가한다.
        //    this.controlBases.Add(control);

        //    return true;
        //}

        private void DeselectAllControls()
        {
            controlBases.ForEach(x => x.Model.Selected = false);
        }
        private void SortControls()
        {
            controlBases = controlBases.OrderBy(x => x.Model.Selected ? 1 : 0).ToList();

            IsSelectedChildControl();

            bool IsSelectedChildControl()
            {
                //foreach (_05_BaseControlModel control in controlBases)
                //{
                //    if (control.Selected)
                //    {
                //        text1 = $"UID      : {control.Uid} {Environment.NewLine}";
                //        text2 = $"Position : {(int)control.X} {(int)control.Y} {(int)control.Right} {(int)control.Bottom}{Environment.NewLine}";
                //        text3 = $"Size     : {(int)control.Width} {(int)control.Height} {Environment.NewLine}";
                //        return true;
                //    }
                //}
                //return false;
                return true;
            }
        }

        public void CreateControl(int x, int y, int width, int height)
        {
            //최소 사이즈 이상 드래그 된 경우만 진행한다. ?? 아니면 작게 그리면 최소사이즈만큼 그려준다?
            int controlMinimumSize = 10;
            if (width < controlMinimumSize || height < controlMinimumSize)
                return;

            //새로 생성하는 컨트롤에 TabIndex를 할상해서 키보드 이벤트를 받도록 한다. 
            int tabIndex = 0;
            if (controlBases.Count > 0)
                tabIndex = controlBases.Max(x => x.TabIndex) + 1;

            //var control = new ControlBase()
            //{
            //    Uid = Guid.NewGuid().ToString(),
            //    X = x,
            //    Y = y,
            //    Width = width,
            //    Height = height,
            //    TabIndex = tabIndex
            //};

            var control = new ControlBase(x, y, width, height, tabIndex);

            //컴포넌트 목록에 추가한다.
            this.controlBases.Add(control);


        }
    }
}

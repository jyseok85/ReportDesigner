using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen.Blazor;
using ReportDesigner.Blazor.Common.Data.EtcComponents;
using ReportDesigner.Blazor.Common.Data.Model;
using ReportDesigner.Blazor.Common.Services;
using System.Drawing;
using System.Reflection.Metadata.Ecma335;
using static ReportDesigner.Blazor.Common.Data.Model.BandModel;

namespace ReportDesigner.Blazor.Common.Data.BaseClass
{
    public class BandBase : ComponentBase
    {
        [Inject]
        ControlCreationService CreationService { get; set; }

        [Inject]
        ControlModificationServcie ModificationServcie { get; set; }
        [Inject]
        SelectedControlService Selectedservice { get; set; }

        [Inject]
        DragAndDropService DragService { get; set; }

        [Inject]
        DesignerOptionService Options { get; set; }


        //public ActionState State { get; set; } = ActionState.None;

        public List<ControlBase> controlBases = new List<ControlBase>();

        public BandModel Model { get; set; } = new BandModel();


        public BandBase()
        {
            this.Model.Type = ReportComponentModel.Control.Band;
            this.Model.BandType = BandModel.Band.Content;
        }
        //public CreationModel creationModel = new CreationModel();
        public void OnPointerUp(PointerEventArgs e)
        {
            //포인트 업은 바디에서 사용.
            //포인트 다운은 밴드 내에서 시작.
            if (Options.State == DesignerOptionService.ActionState.Drag)
            {               

                int mouseMoveDictanceX = (int)(e.ClientX - DragService.MouseX);
                int mouseMoveDictanceY = (int)(e.ClientY - DragService.MouseY);

                ControlBase control = controlBases.Find(x => x.Model.Uid == DragService.Uid);
                Console.WriteLine("Band - OnPointerUp : " + DragService.Uid);

                if (control is not null)
                {
                    int targetX = (int)DragService.PosX;
                    int targetY = (int)DragService.PosY;

                    //여백으로 이동했는지 체크
                    if (targetX < 0)
                    { targetX = 0; }
                    if (targetY < 0)
                    { targetY = 0; }

                    //컨트롤이 밴드의 우측을 벗어난경우(페이퍼용지 사이즈에서 좌우 여백을 뺀다)
                    int bandWidth = Options.PaperSize.Width - Options.PaperMargin.Left - Options.PaperMargin.Right;
                    int bandHeight = Options.PaperSize.Width - Options.PaperMargin.Left - Options.PaperMargin.Right;

                    if (targetX + control.Model.Width > bandWidth)
                    { 
                        targetX = bandWidth - control.Model.Width; 
                    }

                    if (targetY + control.Model.Height > Model.Bottom)
                    {
                        targetY = Model.Height - control.Model.Height;
                    }

                    control.Model.X = targetX;
                    control.Model.Y = targetY;


                    //부모 밴드를 가져온다.
                    
                }   
            }

            return;
        }

        public void OnPointerDown(PointerEventArgs e)
        {
            Console.WriteLine("Bandbase.cs - OnPointerDown");
            //좌측 컨트롤을 클릭하면 생성모드로 진입한다.
            if (Options.State == DesignerOptionService.ActionState.Create)
            {
                Selectedservice.CurrentBand = this;
                //자식 컴포넌트가 선택되지 않았다면 선택된 모든 컴포넌트를 해제해준다.
                DeselectAllControls();

                //2. 생성 모드일 경우(임시로 항상 생성모드로 한다.)
                CreationService.ActionStart(e);
            }
            else if (Options.State == DesignerOptionService.ActionState.Drag)
            {
                //todo 여기 안들어오는데??
                if (Selectedservice.CurrentSelectedModel is not null)
                    DragService.StartDrag(Selectedservice.CurrentSelectedModel, e.ClientX, e.ClientY);
            }
            else
                Selectedservice.OnPointerDown(e, this.Model, this);
        }



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

        private int GetNextTabIndex()
        {
            int tabIndex = 0;
            if (controlBases.Count > 0)
                tabIndex = controlBases.Max(x => x.Model.TabIndex) + 1;
            return tabIndex;
        }
        
        public void CreateControl(int x, int y, int width, int height)
        {
            //최소 사이즈 이상 드래그 된 경우만 진행한다. ?? 아니면 작게 그리면 최소사이즈만큼 그려준다?
            int controlMinimumSize = 10;
            if (width < controlMinimumSize || height < controlMinimumSize)
                return;

            //새로 생성하는 컨트롤에 TabIndex를 할당해서 키보드 이벤트를 받도록 한다. 
            int tabIndex = GetNextTabIndex();
            var control = new ControlBase(x, y, width, height, tabIndex);
            control.Model.ParentUid = this.Model.Uid;
            control.Model.Type = ReportComponentModel.Control.Label;

            //컴포넌트 목록에 추가한다.
            this.controlBases.Add(control);
            Options.AddControl(control.Model.Uid, control.Model);
        }

        /// <summary>
        /// 복사 붙여넣기 할때 사용
        /// </summary>
        public void CreateControl(ReportComponentModel model, Location location = null)
        {
            var control = new ControlBase();
            control.Model = model;
            control.Model.TabIndex = GetNextTabIndex();
            control.Model.ZIndex = control.Model.TabIndex;

            control.Model.Selected = false;
            control.Model.Uid = Guid.NewGuid().ToString();
            //붙여넣기 할때 지정된 위치가 없거나 같은 부모라면 위치를 10,10 만큼 이동시켜준다. 
            if (location == null && control.Model.ParentUid == this.Model.Uid)
            {
                control.Model.X += 10;
                control.Model.Y += 10;
            }
            else
            {
                if (location != null)
                {
                    control.Model.X = location.X;
                    control.Model.Y = location.Y;
                }
                control.Model.ParentUid = this.Model.Uid;
            }
            //컴포넌트 목록에 추가한다.
            this.controlBases.Add(control);
            Options.AddControl(control.Model.Uid, control.Model);

        }
    }
}

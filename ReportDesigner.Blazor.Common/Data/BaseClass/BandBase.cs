using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Newtonsoft.Json.Linq;
using Radzen.Blazor;
using ReportDesigner.Blazor.Common.Data.EtcComponents;
using ReportDesigner.Blazor.Common.Data.Model;
using ReportDesigner.Blazor.Common.Services;
using ReportDesigner.Blazor.Common.Utils;
using System.Drawing;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using static ReportDesigner.Blazor.Common.Data.Model.BandModel;

namespace ReportDesigner.Blazor.Common.Data.BaseClass
{
    public class BandBase : ComponentBase
    {
        [Inject]
        ControlCreationService CreationService { get; set; }

        [Inject]
        ControlResizeService ModificationServcie { get; set; }
        [Inject]
        SelectedControlService SelectedService { get; set; }

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
                Logger.Instance.Write("Band - OnPointerUp : " + DragService.Uid);

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


        public void OnPointerDown(PointerEventArgs e, string value = null)
        {
            if (Options.EventStartObject != null)
                return;                  
            Options.EventStartObject = this;
            Logger.Instance.Write("");

            SelectedService.CurrentBand = this;

            //좌측 컨트롤을 클릭하면 생성모드로 진입한다.
            if (Options.State == DesignerOptionService.ActionState.Create)
            {
                //자식 컴포넌트가 선택되지 않았다면 선택된 모든 컴포넌트를 해제해준다.
                DeselectAllControls();

                //2. 생성 모드일 경우(임시로 항상 생성모드로 한다.)
                CreationService.ActionStart(e);
            }
            else if (Options.State == DesignerOptionService.ActionState.Drag)
            {
                //todo 여기 안들어오는데??
                if (SelectedService.CurrentSelectedModel is not null)
                    DragService.StartDrag(SelectedService.CurrentSelectedModel, e.ClientX, e.ClientY);
            }
            else
                SelectedService.OnPointerDown(e, this.Model, this);
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
        
        public void AddControl(ControlBase control)
        {
            int tabIndex = GetNextTabIndex();

            control.Model.TabIndex = tabIndex;
            control.Model.ZIndex = tabIndex;
            control.Model.ParentUid = this.Model.Uid;

            //컴포넌트 목록에 추가한다.
            this.controlBases.Add(control);
            Options.AddControl(control.Model.Uid, control.Model);
        }

        /// <summary>
        /// 복사 붙여넣기 할때 사용
        /// 컨텍스트 메뉴로 붙여넣기 할때 위치 정보가 들어간다. 
        /// </summary>
        public void AddControl(ControlBase control, Location location = null)
        {
 
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

        public void RemoveSelectedControl()
        {
            var uid = SelectedService.CurrentSelectedModel.Uid;
            var control = controlBases.Find(x => x.Model.Uid == uid);

            if (control != null)
            {
                this.controlBases.Remove(control);
                Options.RemoveControl(uid);
            }
        }
    }
}

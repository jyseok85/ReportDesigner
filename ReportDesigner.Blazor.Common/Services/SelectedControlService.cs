using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using ReportDesigner.Blazor.Common.Data.BaseClass;
using ReportDesigner.Blazor.Common.Data.Model;
using ReportDesigner.Blazor.Common.UI.ReportControls.Controls;
using ReportDesigner.Blazor.Common.Utils;
using System.Reflection;

namespace ReportDesigner.Blazor.Common.Services
{
    
    public class SelectedControlService
    {
        [Inject]
        public required DesignerCSSService CSS { get; set; }
        [Inject]
        public required IJSRuntime JsRuntime { get; set; }

        enum Type
        { 
            report,
            layer,
            band,
            control
        }
     
        

        private List<ReportComponentModel> models = new List<ReportComponentModel>();
        public List<ReportComponentModel> Models => models;

        //todo : 쓸지 말지 아직 모르겠음.
        private object razorComponent;
        public object RazorComponent => razorComponent;

        /// <summary>
        /// 변경시점 - 컨트롤을 생성할때, 밴드를 클릭할때
        /// </summary>
        public BandBase? CurrentBand { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="model">선택한 오브젝트</param>
        /// <param name="razorComponent">현재 Razor 오브젝트</param>
        public void OnPointerDown(PointerEventArgs e, ReportComponentModel model, object razorComponent = null)
        {
            SelectControl(e.CtrlKey, model, razorComponent);
        }
        public void SelectControl(bool isMultiSelect, ReportComponentModel model, object parentBand = null)
        {
            BeforeSelectedModel = CurrentSelectedModel; 
            if (isMultiSelect == false)
            {
                models.ForEach(x => x.Selected = false);
                models.Clear();
            }
            models.Add(model);
            model.Selected = true;
            
            CurrentSelectedModel = model;

            if (parentBand is BandBase)
            {
                this.CurrentBand = parentBand as BandBase;
            }

            this.razorComponent = parentBand;

        }
        public ReportComponentModel BeforeSelectedModel { get; set; }
        public ReportComponentModel LastSelectModel => models[models.Count - 1];
       
        public async void ApplyResize(int x, int y, int width, int height, ReportComponentModel parent)
        {
            LastSelectModel.X += x;
            LastSelectModel.Y += y;

            if (LastSelectModel.X < 0)
            {
                width += LastSelectModel.X;
                LastSelectModel.X = 0;
                LastSelectModel.AbsoluteOffsetX = parent.AbsoluteOffsetX;
            }
            if (LastSelectModel.Y < 0)
            {
                height += LastSelectModel.Y;
                LastSelectModel.Y = 0;
                LastSelectModel.AbsoluteOffsetY = parent.AbsoluteOffsetY;
            }


            //오른쪽 밴드 이후 영역으로 나가는지 체크
            if (x >= 0)
            {
                if (width + LastSelectModel.AbsoluteOffsetX > parent.Right + parent.AbsoluteOffsetX)
                {
                    int 변경하는가로사이즈 = width + LastSelectModel.AbsoluteOffsetX;
                    int 부모밴드의오른쪽좌표 = parent.Right + parent.AbsoluteOffsetX;
                    int diff = 변경하는가로사이즈 - 부모밴드의오른쪽좌표;
                    width -= diff;
                }
            }

            string msg = $"X:{LastSelectModel.X}, Width:{width}";
            Logger.Instance.Write(msg);

            int minimumWidth = CSS.DefaultPadding * 2;
            int minimumHeight = CSS.DefaultPadding * 2;

            
            if (LastSelectModel.Type == ReportComponentModel.Control.Table)
            {
                minimumWidth = (minimumWidth * LastSelectModel.TableInfo.ColCount) + 1;
                minimumHeight = (minimumHeight * LastSelectModel.TableInfo.RowCount) + 4;

                //var size = await JsRuntime.InvokeAsync<Dictionary<string,float>>("getDivSize", LastSelectModel.Uid);

                //minimumWidth = (int)size["width"];
                //minimumHeight = (int)size["height"];
            }

            if (minimumWidth > width)
                width = minimumWidth;

            if (minimumHeight > height)
                height = minimumHeight;

            //일반 컨트롤의 경우 모델사이즈를 변경하고, 리프레시를 해주면 반영되지만.
            //테이블의 경우 각 셀의 사이즈에 따라서 외부 Tr 의 사이즈가 변경된다..
            if (LastSelectModel.Type == ReportComponentModel.Control.Table)
            {
                LastSelectModel.TableInfo.UpdateCellSize(width, height); 
            }


          

            LastSelectModel.Width = width;
            LastSelectModel.Height = height;
        }

        public required ReportComponentModel CurrentSelectedModel { get; set; } = new ReportComponentModel();

        public void SetEditMode()
        {
            CurrentSelectedModel.IsEditMode = true;
        }

        public ReportComponentModel? CopiedModel = new ReportComponentModel();

        public void CopyControl()
        {
            CopiedModel = CurrentSelectedModel.DeepClone();
            Logger.Instance.Write("Control Copied");

        }


        public async Task UpdateInnerTextControlScale()
        {

            //선택된 오브젝트의 UID로 클라이언트의 사이즈를 가져온다.
            var value = await JsRuntime.InvokeAsync<float>("GetInnerTextWidth", this.LastSelectModel.Uid);

            int scale = (int)(LastSelectModel.Width / value * 100);

            var paragraph = this.LastSelectModel.Paragraph;

            int targetScale = scale;
            if (paragraph.MaxScale > scale && paragraph.MinScale < scale)
            {
                targetScale = scale;
            }
            else if (paragraph.MaxScale < scale)
            {
                targetScale = paragraph.MaxScale;
            }
            else if (paragraph.MinScale > scale)
            {
                targetScale = paragraph.MinScale;
            }

            if(targetScale < paragraph.MinScale)
            {
                targetScale = paragraph.MinScale;
            }   

            if(targetScale > paragraph.MaxScale)
            {
                targetScale = paragraph.MaxScale;
            }   

            paragraph.CurrentScale = targetScale;

            Console.WriteLine($"Ratio : {paragraph.CurrentScale}");


            //this.selectedControlService.LastSelectModel.Paragraph.CurrentScale = (int)(this.Width / value * 100);

        }


    }
}

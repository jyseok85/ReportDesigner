using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using ReportDesigner.Blazor.Common.Data.BaseClass;
using ReportDesigner.Blazor.Common.Data.Model;
using ReportDesigner.Blazor.Common.Utils;
using System.Collections.Generic;

namespace ReportDesigner.Blazor.Common.Services
{

    public class SelectionService
    {
        [Inject]
        public required DesignerOptionService Options { get; set; }
        [Inject]

        public required DesignerCSSService CSS { get; set; }
        [Inject]
        public required IJSRuntime JsRuntime { get; set; }

        public SelectionService(DesignerOptionService optionService, DesignerCSSService css, IJSRuntime jsRuntime)
        {
            this.Options = optionService;
            this.CSS = css;
            this.JsRuntime = jsRuntime;
        }

        private enum Type
        {
            report,
            layer,
            band,
            control
        }


        //models 를 생성하지 않고 models 를 초기화하는 방식으로 변경하면 readonly 한정자를 사용할 수 있을 것 같습니다.
        private readonly List<ReportComponentModel> models = [];
        public List<ReportComponentModel> Models => models;

        //todo : 쓸지 말지 아직 모르겠음.
        private object? razorComponent;
        public object? RazorComponent => razorComponent;

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
        public void OnPointerDown(PointerEventArgs e, ReportComponentModel model, object? razorComponent = null)
        {
            SelectControl(e.CtrlKey, model, razorComponent);
        }
        public void SelectControl(bool isMultiSelect, ReportComponentModel model, object? parentBand = null)
        {
            BeforeSelectedModel = currentSelectedModel;
            if (isMultiSelect == false)
            {
                models.ForEach(x => x.Selected = false);
                models.Clear();
            }
            models.Add(model);
            model.Selected = true;

            this.currentSelectedModel = model;

            if (parentBand is BandBase)
            {
                CurrentBand = parentBand as BandBase;
            }

            this.razorComponent = parentBand;

        }
        public required ReportComponentModel BeforeSelectedModel { get; set; }
        public ReportComponentModel LastSelectModel => models[models.Count - 1];
        public ReportComponentModel CurrentSelectedModel
        {
            get
            {
                return this.currentSelectedModel;
            }
            set
            {
                this.currentSelectedModel = value;
            }
        }

        private ReportComponentModel currentSelectedModel = new();


        //public async void ApplyResize(int x, int y, int width, int height, ReportComponentModel parent)
        //{
        //    LastSelectModel.X += x;
        //    LastSelectModel.Y += y;

        //    if (LastSelectModel.X < 0)
        //    {
        //        width += LastSelectModel.X;
        //        LastSelectModel.X = 0;
        //        LastSelectModel.AbsoluteOffsetX = parent.AbsoluteOffsetX;
        //    }
        //    if (LastSelectModel.Y < 0)
        //    {
        //        height += LastSelectModel.Y;
        //        LastSelectModel.Y = 0;
        //        LastSelectModel.AbsoluteOffsetY = parent.AbsoluteOffsetY;
        //    }


        //    //오른쪽 밴드 이후 영역으로 나가는지 체크
        //    if (x >= 0)
        //    {
        //        if (width + LastSelectModel.AbsoluteOffsetX > parent.Right + parent.AbsoluteOffsetX)
        //        {
        //            var 변경하는가로사이즈 = width + LastSelectModel.AbsoluteOffsetX;
        //            var 부모밴드의오른쪽좌표 = parent.Right + parent.AbsoluteOffsetX;
        //            var diff = 변경하는가로사이즈 - 부모밴드의오른쪽좌표;
        //            width -= diff;
        //        }
        //    }

        //    var msg = $"X:{LastSelectModel.X}, Width:{width}";
        //    Logger.Instance.Write(msg);

        //    var minimumWidth = CSS.GlobalPadding * 2;
        //    var minimumHeight = CSS.GlobalPadding * 2;


        //    if (LastSelectModel.Type == ReportComponentModel.Control.Table)
        //    {
        //        minimumWidth = (minimumWidth * LastSelectModel.TableInfo.ColCount) + 1;
        //        minimumHeight = (minimumHeight * LastSelectModel.TableInfo.RowCount) + 4;

        //        //var size = await JsRuntime.InvokeAsync<Dictionary<string,float>>("getDivSize", LastSelectModel.Uid);

        //        //minimumWidth = (int)size["width"];
        //        //minimumHeight = (int)size["height"];
        //    }

        //    if (minimumWidth > width)
        //        width = minimumWidth;

        //    if (minimumHeight > height)
        //        height = minimumHeight;

        //    //일반 컨트롤의 경우 모델사이즈를 변경하고, 리프레시를 해주면 반영되지만.
        //    //테이블의 경우 각 셀의 사이즈에 따라서 외부 Tr 의 사이즈가 변경된다..
        //    if (LastSelectModel.Type == ReportComponentModel.Control.Table)
        //    {
        //        LastSelectModel.TableInfo.UpdateCellSize(width, height);
        //    }

        //    LastSelectModel.Width = width;
        //    LastSelectModel.Height = height;
        //}


        public ReportComponentModel? CopiedModel = new();

        public void CopyControl()
        {
            CopiedModel = this.currentSelectedModel.DeepClone();
            Logger.Instance.Write("Control Copied");
        }

        //문제점1. 텍스트를 변경하고 DIV 사이즈가 변경되어야 스케일을 조절할 수 있다.
        //즉 입력 컨틀롤 -> 모델에 데이터 바인딩 -> 텍스트표시 컨트롤 -> 이후 사이즈조절 순으로 가능하며
        //결국 텍스트 표시 컨트롤이 갱신되어야 가능하다(결국 Body 가 갱신되어야 한다)
        //그래픽객체에서 measureText 를 사용해서 사이즈를 계산할 수 있다면, 사전에 스케일을 조절 할 수 있지만, 컨트롤의 사이즈를 가져오는 방향으로는 불가능하다. 
        //즉 UI가 깔끔하게 동작할 수 없다. 
        //대안1. 에디트 모드가 끝나면 사이즈조절을 False로 변경한다.(텍스트가 변경되었다면) 그리고 다시 사이즈 조절을 수동으로 On으로 변경한다.
        //대안2. 그냥 느리게 모든 액션이 끝난다음에 다시 스케일조절 로직을 동작시킨다. 



        /// <summary>
        /// 1. 내부 컨트롤의 사이즈가 변경되었을 때.
        /// 2. 우측 사이드 바에서 옵션을 변경할때.
        /// 3. 컨트롤의 사이즈를 변경할때.
        /// </summary>
        /// </history>
        /// <param name="width"></param>
        /// <returns></returns>
        public async Task UpdateInnerTextControlScale(int width = 0)
        {
            var target = this.currentSelectedModel;
            //todo : 현재 밴드만 되어 있지만 추후 다른 컨트롤 예외처리 할 수도 있음.
            if (target.Type == ReportComponentModel.Control.Band)
            {
                Logger.Instance.Write("Type is Band. Select Before Control.");
                target = BeforeSelectedModel;
            }
            Logger.Instance.Write($"Uid : {target.Uid}");


            //선택한 오브젝트에 텍스트가 없는 경우
            if (target.Text == string.Empty)
            {
                Logger.Instance.Write("Text is Empty");
                return;
            }

            //테이블일 경우에는 텍스트가 당연히 없다. 그래도 혹시 모르니 추가한다. 
            if (target.Type == ReportComponentModel.Control.Table)
            {
                Logger.Instance.Write("Type is Table");
                return;
            }

            //텍스트가 자동조절이 아닐경우에는 하지말자.
            if (target.Paragraph.AutoFitText == false)
            {
                Logger.Instance.Write("AutoFitText is False");
                return;
            }


            //선택된 오브젝트의 UID로 클라이언트의 사이즈를 가져온다.
            var value = await JsRuntime.InvokeAsync<float>("GetInnerTextWidth", target.Uid);

            //내부 텍스트 사이즈가 0인 경우는 무시한다.
            if (value == 0)
            {
                Logger.Instance.Write("InnerTextWidth is 0");
                return;
            }

            //변경될 폭이 0인 경우(넘어온 값이 없는 경우) 현재 선택된 모델의 폭을 사용한다.
            if (width == 0)
            {
                width = target.Width;
            }


            var targetWidth = (width - (CSS.GlobalPadding * 2));
            var scale = (int)(targetWidth / value * 100);

            var paragraph = target.Paragraph;

            var targetScale = scale;
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

            if (targetScale < paragraph.MinScale)
            {
                targetScale = paragraph.MinScale;
            }

            if (targetScale > paragraph.MaxScale)
            {
                targetScale = paragraph.MaxScale;
            }

            paragraph.CurrentScale = targetScale;

            Logger.Instance.Write($"Ratio : {paragraph.CurrentScale}");


            //this.selectedControlService.LastSelectModel.Paragraph.CurrentScale = (int)(this.Width / value * 100);

            Options.RefreshBody();

        }
    
    }
}

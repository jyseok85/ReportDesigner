using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using ReportDesigner.Blazor.Common.Data.BaseClass;
using ReportDesigner.Blazor.Common.Data.EtcComponents;
using ReportDesigner.Blazor.Common.Data.Model;
using ReportDesigner.Blazor.Common.UI.ReportControls.Controls;
using ReportDesigner.Blazor.Common.Utils;
using System.Collections.Generic;
using System.Numerics;

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

        public ReportComponentModel? CopiedModel = new();

        public ReportComponentModel EditedControl { get; set; }
        public void CopyControl()
        {
            CopiedModel = this.currentSelectedModel.DeepClone();
            Logger.Instance.Write("Control Copied");
        }



        //마지막으로 입력된 컨트롤을 가져온다.    
        public ReportComponentModel GetResizeTarget()
        {
            if (this.EditedControl == null)
            {
                if (CurrentSelectedModel.Type == ReportComponentModel.Control.Table)
                {
                    return CurrentSelectedModel;
                }
                else
                {
                    //확인필요
                    Logger.Instance.Write("EditedControl is null", Microsoft.Extensions.Logging.LogLevel.Warning);
                    return null;
                }
            }
            else
            {
                //1.선택한 컨트롤과 수정된 컨트롤이 같은경우
                if (this.currentSelectedModel.Uid == this.EditedControl.Uid)
                {
                    return this.EditedControl;
                }

                //2.선택한 컨트롤과 수정된 컨트롤이 다른경우
                //  그러나 이전 선택한 컨트롤이 수정된 컨트롤인 경우
                if (this.BeforeSelectedModel.Uid == this.EditedControl.Uid)
                {
                    return this.EditedControl;
                }
                else
                {
                    Logger.Instance.Write("EditedControl is null", Microsoft.Extensions.Logging.LogLevel.Warning);
                    return null;
                }
            }
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
        /// - 텍스트 글자 변경될때가 아님.
        /// </summary>
        /// </history>
        /// <param name="width"></param>
        /// <returns></returns>
        public async Task UpdateInnerTextControlScale(int width = 0)
        {
            var target = GetResizeTarget();
                
            if(target == null)
            {
                Logger.Instance.Write("target is null", Microsoft.Extensions.Logging.LogLevel.Trace);
                return;
            }

            //todo : 현재 밴드만 되어 있지만 추후 다른 컨트롤 예외처리 할 수도 있음.
            Logger.Instance.Write("Options.State :" + Options.State.ToString());

            //테이블일 경우에는 텍스트가 당연히 없다. 그래도 혹시 모르니 추가한다. 
            if (target.Type == ReportComponentModel.Control.Table)
            {
                //자식 오브젝트를 전부 업데이트 해줘야 한다....
                foreach (var child in target.Children)
                {
                    if (child.Paragraph.AutoFitText)
                    {
                        await UpdateScale(child);
                    }
                }
            }
            else
            {
                if (target.Paragraph.AutoFitText)
                {
                    Logger.Instance.Write($"Uid : {target.Uid}");
                    await UpdateScale(target, width);
                }
            }    
        }

        public async Task UpdateScale(ReportComponentModel target, int width = 0)
        {
            //선택한 오브젝트에 텍스트가 없는 경우
            if (target.Text == string.Empty)
            {
                Logger.Instance.Write("Text is Empty" , Microsoft.Extensions.Logging.LogLevel.Warning);
                return;
            }
            //선택된 오브젝트의 UID로 클라이언트의 사이즈를 가져온다.
            var value = await JsRuntime.InvokeAsync<ComponentTextSize>("GetInnerTextWidth", target.Uid);
            if (value == null)
            {
                Logger.Instance.Write("value is null", Microsoft.Extensions.Logging.LogLevel.Warning);
                return;
            }

            var outer = (int)value.outer;
            var inner = (int)value.inner;
            //내부 텍스트 사이즈가 0인 경우는 무시한다.
            if (inner == 0)
            {
                Logger.Instance.Write("InnerTextWidth is 0" , Microsoft.Extensions.Logging.LogLevel.Warning);
                return;
            }

            //변경될 폭이 0인 경우(넘어온 값이 없는 경우) 현재 선택된 모델의 폭을 사용한다.
            if (width == 0)
            {
                width = target.Width;
            }

            if (target.Type == ReportComponentModel.Control.TableCell)
            {
                width = outer;

            }

            var targetWidth = (width - (CSS.GlobalPadding * 2));
            var scale = (int)((targetWidth * 100) / inner);
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

            if (target.Type == ReportComponentModel.Control.TableCell)
                Logger.Instance.Write($"{target.Type} - R:{target.TableCellInfo.Row} C:{target.TableCellInfo.Col} Ratio: {paragraph.CurrentScale}");


        }


        
    }

    public class ComponentTextSize
    {
        public double outer { get; set; }
        public double inner { get; set; }
    }

}

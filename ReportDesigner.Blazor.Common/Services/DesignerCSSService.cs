using Radzen;
using Radzen.Blazor;
using ReportDesigner.Blazor.Common.Data;
using ReportDesigner.Blazor.Common.Data.Model;
using ReportDesigner.Blazor.Common.Utils;
using System.Drawing;

namespace ReportDesigner.Blazor.Common.Services
{
    public class DesignerCSSService
    {
        public Orientation Orientation { get; set; } =Orientation.Horizontal;
        public AlignItems AlignItems { get; set; } = AlignItems.End;
        public JustifyContent JustifyContent { get; set; } = JustifyContent.SpaceBetween;
        public FlexWrap Wrap { get; set; } = FlexWrap.NoWrap;
        public string Gap { get; set; } = "10px";
        public bool Reverse { get; set; } = false;

        public string DropDownPopupStyle { get; set; } = @" --rz-dropdown-item-font-size: 0.7rem;
        --rz-dropdown-item-padding: 0.4rem 0.5rem;--rz-input-height: 1.8rem;
        --rz-input-font-size: 0.7rem;";


        public bool ShowHSV { get; set; } = true;
        public bool ShowRGBA { get; set; } = true;
        public bool ShowColors { get; set; } = false;
        public bool ShowButton { get; set; } = true;

        /// <summary>
        /// 전역 컨트롤 패딩 
        /// </summary>
        public int GlobalPadding { get; set; } = 10;

        /// <summary>
        /// 테이블 셀의 최소 간격
        /// </summary>
        public int GridCellMinimumSize => GlobalPadding * 2;
        public string GetDynamicTableTextInner(ReportComponentModel model)
        {
            return GetModelStyle("textarea.width", model) +
            GetModelStyle("textarea.height", model) +
            GetModelStyle("font-size", model) +
            GetModelStyle("font-family", model) +
            GetModelStyle("padding", model) +
            GetModelStyle("line-height", model);
        }

        public string GetModelStyle(string type, ReportComponentModel model)
        {
            string name = type.ToLower();
            switch (name)
            {
                case "translate":
                    return "translate(" + model.X + "px, " + model.Y + "px);";
                case "width":
                    return "width:" + (model.Width) + "px;";
                case "height":
                    return "height:" + (model.Height) + "px;";
               
                case "border":
                    if (model.IsOverlap == false)
                    {
                        if (model.Border.Use)
                        {
                            if (model.Type == ReportComponentModel.Control.TableCell)
                            {
                                //모델이 선택했는지에 따라서 테두리 색상을 변경한다.
                                if (model.Selected)
                                {
                                    string border =
                                        $"{name}-top: {model.Border.Thickness}px {model.Border.Style} red;" +
                                        $"{name}-right: {model.Border.Thickness}px {model.Border.Style} red;" +
                                        $"{name}-bottom: {model.Border.Thickness}px {model.Border.Style} red;" +
                                        $"{name}-left: {model.Border.Thickness}px {model.Border.Style} red;";
                                    return border;
                                }                                   
                                else
                                {
                                    if (model.Border.UseIndividualBorders)
                                        return
                                        $"{name}-top: {model.Border.Thickness}px {model.Border.Style} {model.Border.TopColor};" +
                                        $"{name}-right: {model.Border.Thickness}px {model.Border.Style} {model.Border.RightColor};" +
                                        $"{name}-bottom: {model.Border.Thickness}px {model.Border.Style} {model.Border.BottomColor};" +
                                        $"{name}-left: {model.Border.Thickness}px {model.Border.Style} {model.Border.LeftColor};";
                                    else
                                        return $"{name}: {model.Border.Thickness}px {model.Border.Style} {model.Border.Color};";
                                }
                            }
                            else
                            {
                                if (model.Border.UseIndividualBorders)
                                    return
                                    $"{name}-top: {model.Border.Thickness}px {model.Border.Style} {model.Border.TopColor};" +
                                    $"{name}-right: {model.Border.Thickness}px {model.Border.Style} {model.Border.RightColor};" +
                                    $"{name}-bottom: {model.Border.Thickness}px {model.Border.Style} {model.Border.BottomColor};" +
                                    $"{name}-left: {model.Border.Thickness}px {model.Border.Style} {model.Border.LeftColor};";
                                else
                                    return $"{name}: {model.Border.Thickness}px {model.Border.Style} {model.Border.Color};";
                            }
                        }
                        else
                        {
                            //todo : 디자이너 or 뷰어 모드에 따라서 보일지말지 처리
                            //return $"{name}: {model.Border.Thickness}px {model.Border.Style} transparent;";
                            //테이블이면 그냥 투명해야하네..
                            if(model.Type == ReportComponentModel.Control.TableCell)
                                return $"{name}: {model.Border.Thickness}px {model.Border.Style} transparent;";

                            return $"{name}: {model.Border.Thickness}px dotted lightgray;";
                        }
                    }
                    else
                    {
                        return $"{name}: {model.Border.Thickness}px {model.Border.Style} red;";
                    }
                    break;
             
                case "background-color":
                    {
                        if (model.Type == ReportComponentModel.Control.TableCell)
                        {    
                            return $"{type.ToLower()} : {model.BackColor}; ";
                        } 
                        else
                            return $"{type.ToLower()} : {model.BackColor}; ";
                    }
                case "font-family":
                    return $"{type.ToLower()} : {model.Font.FontFamily}; ";
                case "font-size":
                    return $"{type.ToLower()} : {model.Font.FontSize}px; ";
                case "font-style":
                    return $"{model.Font.FontStyle};";
                case "align-items": //수직정렬
                    return $"{type.ToLower()} : {model.Paragraph.VerticalAlignment}; ";
                case "vertical-align":
                    return $"{type.ToLower()} : {model.Paragraph.VerticalAlignment.Replace("start", "top").Replace("center", "middle").Replace("end", "bottom")}; ";
                case "justify-content": //수평정렬
                    return $"{type.ToLower()} : {model.Paragraph.HorizontalAlignment.Replace("justify","normal")}; ";
                case "text-align":
                    return $"{type.ToLower()} : {model.Paragraph.HorizontalAlignment}; ";
                case "color":
                    return $"{type.ToLower()} : {model.Font.FontColor}; ";
                case "z-index":
                    if (model.Type == ReportComponentModel.Control.TableCell && model.Selected)
                    {
                        return $"{type.ToLower()} : {model.ZIndex + 1000}; ";
                    }
                    else
                        return $"{type.ToLower()} : {model.ZIndex}; ";
               

                case "textarea.width":
                    return "width:" + (model.Width - 1) + "px;";
                case "textarea.height":
                    return "height:" + (model.Height - 1) + "px;";

                case "padding":
                case "margin":
                    return $"{type.ToLower()} : {this.GlobalPadding}px; ";
                case "textarea.padding":
                    return $"padding : {this.GlobalPadding}px; ";

      
                case "line-height":
                    return $"{type.ToLower()} : {model.Paragraph.LineHeight}; ";
                case "transform-origin":
                    {
                        if (model.Paragraph.AutoFitText)
                        {
                            var style = $"{type.ToLower()} : {model.Paragraph.HorizontalAlignment.Replace("start", "left").Replace("end", "right")}; ";
                            return style;
                        }
                        else
                            return "";
                    }
                case "transform":
                    {
                        if (model.Paragraph.AutoFitText)
                        {
                            var scaleX = model.Paragraph.CurrentScale * 0.01f;
                            return $"{type.ToLower()} : scaleX({scaleX}); ";
                        }
                        else
                            return "";
                    }
                //case "overflow":
                //    {
                //        if (model.Paragraph.AutoFitText == false)
                //        {
                //            return $"{type.ToLower()} : hidden; ";
                //        }
                //        else
                //        {
                //            //텍스트 스케일을 조절한 경우에 전부 표시하지 않으면
                //            return $"{type.ToLower()} : visible; ";
                //        }
                //    }
               

                case "overflow-text":
                    {
                        var style = string.Empty;
                        switch(model.Paragraph.OverFlowText)
                        {
                            case OverFlowText.Visible:
                                style= "overflow: visible;";
                                break;
                            case OverFlowText.Clip:
                                style = "overflow: hidden;";
                                break;
                            case OverFlowText.Ellipsis:
                                style = "overflow: hidden; text-overflow: ellipsis;";
                                break;
                            case OverFlowText.WordWrap:
                                style = "overflow: hidden; white-space: normal; overflow-wrap: break-word;";
                                break;
                        }
                        if (model.Paragraph.OverFlowText == OverFlowText.WordWrap)
                        {
                            if (model.Paragraph.MultiLine)
                                style += "white-space:pre-wrap;";
                        }
                        else
                        {
                            if (model.Paragraph.MultiLine)
                            {
                                style += "white-space: pre;";
                            }
                            else
                            {
                                style += "white-space: nowrap;";
                            }
                        }

                        return style;

                       

                        //보이기 1
                        //overflow visible

                        //전부 자르기 2,3
                        //white-space: normal;
                        //overflow-wrap: normal;  
                        //text-overflow: clip or ellipsis

                        //4 word-wrap 하기 

                        //멀티라인 적용시
                        //좌우는 자르고 상하만 보이기
                        //multiline + ellipsis or multiline + clip

                    }
                case "letter-spacing":
                    return $"{type.ToLower()} :{model.Paragraph.CharacterSpacing}px; ";
                default:
                    return string.Empty;
            }
        }
    }
}

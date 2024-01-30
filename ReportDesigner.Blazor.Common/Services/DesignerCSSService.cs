using Radzen;
using ReportDesigner.Blazor.Common.Data.Model;

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

        public int DefaultPadding { get; set; } = 10;

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
                            if (model.Border.UseIndividualBorders)
                                return
                                $"{name}-top: {model.Border.Thickness}px {model.Border.Style} {model.Border.TopColor};" +
                                $"{name}-right: {model.Border.Thickness}px {model.Border.Style} {model.Border.RightColor};" +
                                $"{name}-bottom: {model.Border.Thickness}px {model.Border.Style} {model.Border.BottomColor};" +
                                $"{name}-left: {model.Border.Thickness}px {model.Border.Style} {model.Border.LeftColor};";
                            else
                                return $"{name}: {model.Border.Thickness}px {model.Border.Style} {model.Border.Color};";
                        }
                        else
                        {
                            //todo : 디자이너 or 뷰어 모드에 따라서 보일지말지 처리
                            //return $"{name}: {model.Border.Thickness}px {model.Border.Style} transparent;";

                            return $"{name}: {model.Border.Thickness}px dotted lightgray;";
                        }
                    }
                    else
                    {
                        return $"{name}: {model.Border.Thickness}px {model.Border.Style} red;";
                    }
                
                case "word-break":
                    return ""; //todo : 고급기능에서 해제가능하게 해야 한다. 
                               // return "word-break:break-all;";
                case "background-color":
                    return $"{type.ToLower()} : {model.BackColor}; ";
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
                case "text-align":
                    return $"{type.ToLower()} : {model.Paragraph.HorizontalAlignment}; ";
                case "color":
                    return $"{type.ToLower()} : {model.Font.FontColor}; ";
                case "z-index":
                    return $"{type.ToLower()} : {model.ZIndex}; ";
               

                case "textarea.width":
                    return "width:" + (model.Width - 1) + "px;";
                case "textarea.height":
                    return "height:" + (model.Height - 1) + "px;";

                case "padding":
                    return $"{type.ToLower()} : {this.DefaultPadding}px; ";
                case "textarea.padding":
                    return $"padding : {this.DefaultPadding}px; ";

                case "white-space":
                    return $"{type.ToLower()} : {(model.Paragraph.AutoLineBreak ? "normal" : "pre")}; ";
                case "line-height":
                    return $"{type.ToLower()} : {model.Paragraph.LineHeight}; ";
                default:
                    return string.Empty;
            }
        }
    }
}

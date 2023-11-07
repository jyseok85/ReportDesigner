using Radzen;

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
    }
}

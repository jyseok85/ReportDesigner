using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.Extensions.Options;
using ReportDesigner.Blazor.Common.Data.EtcComponents;
using ReportDesigner.Blazor.Common.Services;
using ReportDesigner.Blazor.Common.UI.ReportControls.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace ReportDesigner.Blazor.Common.Data.Model
{
    public class ReportComponentModel
    {
        public enum Control
        {
            None,
            Label,
            Table,
            TableCell,
            Band,
            Layer,
            Report
        }
        public Control Type { get; set; } = Control.None;

        public bool Locked { get; set; } = false;

        public ReportComponentModel() {
            this.Uid = Guid.NewGuid().ToString();
        }
        public int TabIndex { get; set; } = 0;

        public string ParentUid { get; set; } = string.Empty;
        public string Uid { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Height { get; set; } = 0;
        public int Width { get; set; } = 0;

        public bool IsGroup { get { return Name != null; } }

        public int ZIndex { get; set; }
        /// <summary>
        /// 일반 X, Y 값은 Div에 그려질 상대값이 설정되며
        /// Absolute는 Report 기준으로 생성되는 절대값을 표현한다. 
        /// 업데이트 시점 : 
        /// 1. 처음 추가될때
        /// 2. 리사이즈 될때
        /// 3. 마우스 업 이벤트 발생할때
        /// </summary>
        public int AbsoluteOffsetX { get; set; } = 0;
        public int AbsoluteOffsetY { get; set; } = 0;
        public int AbsoluteOffsetRight { get; set; } = 0;
        public int AbsoluteOffsetBottom { get; set; } = 0;

        public virtual int X { get; set; } = 0;
        public virtual int Y { get; set; } = 0;
        public virtual int Right { get { return X + Width; } set { } }
        public virtual int Bottom { get { return Y + Height; } set { } }
        private bool selected = false;
        public bool Selected
        {
            get { return this.selected; }
            set
            {
                this.selected = value;
                if (value == true)
                {
                }
                else
                {
                }
            }
        }

        public bool IsEditMode { get; set; } = false;

        public bool Visible
        {
            get { return !Hidden; }
            set { Hidden = !value; }
        }
        public bool Hidden { get; set; } = false;
        public Margin Margin { get; set; } = new Margin();

        public bool IsOverlap { get; set; } = false;


        public string BackColor { get; set; } = "rgba(0,0,0,0)";
        public string ForeColor { get; set; } = "Black";

        public Border Border { get; set; } = new Border();

        public string Text { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;

        public Font Font { get; set; } = new Font();

        public Paragraph Paragraph { get; set; } = new Paragraph();

        public ReportComponentModel? Parent = null;
        public List<ReportComponentModel>? Children { get; set; } = new List<ReportComponentModel>();

        public TableInfo TableInfo { get; set; }
        public TableCellInfo TableCellInfo { get; set; } = null;
        /// <summary>
        /// C#에서는 깊은복사(참조복사)가 없다. 왜 아직도?? 왜?? 느려도 만들어줘야지..
        /// 그래서 얕은복사(값만복사) 이 후에 참조된값들을 각각 복사한다.
        /// </summary>
        /// <returns></returns>
        public ReportComponentModel DeepClone()
        {
            var thisModel = (ReportComponentModel)this.MemberwiseClone();
            thisModel.Margin = (Margin)Margin.Clone();
            thisModel.Border = (Border)Border.Clone();
            thisModel.Font = (Font)Font.Clone();
            thisModel.Paragraph = (Paragraph)Paragraph.Clone();

            if (thisModel.Type == Control.Table)
            {
                //todo : DeepClone - Children 이 있다면 그것도 복사해줘야 함.
            }
            return thisModel;
        }
    }

    public class TableInfo
    {
        public int RowCount { get; set; }
        public int ColCount { get; set; }   

        public Dictionary<int, int> ColWidths { get; set; } = [];
        public Dictionary<int, int> RowHeights { get; set; } = [];

        public Dictionary<int, int> ColPositions { get; set; } = [];
        public Dictionary<int, int> RowPositions { get; set; } = [];
    }

    public class TableCellInfo
    {
        //public TableCell RazorCellRef { get; set; }
        public int Row { get; set; } = 0;
        public int Col { get; set; } = 0;
        //세로 병합
        public int RowSpan { get; set; } = 1;
        public int ColSpan { get; set; } = 1;

        /// <summary>
        /// 컨텐츠가 셀 영역을 넘어갔을 경우 자동으로 높이를 늘릴지 여부
        /// 단락의 멀티라인, 워드랩 이 있을 경우에만 적용된다. 
        /// </summary>
        public bool AutoHeightIncrease { get; set; } = true;


        /// <summary>
        /// 드래그해서 셀을 선택할때 사용하는 변수
        /// </summary>
        public bool IsMultiSelected { get; set; } = false;
    }
}

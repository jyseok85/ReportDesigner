using Microsoft.AspNetCore.Components.Web.Virtualization;
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
            var thisModel       = (ReportComponentModel) this.MemberwiseClone();
            thisModel.Margin    = (Margin)Margin.Clone();
            thisModel.Border    = (Border)Border.Clone();
            thisModel.Font      = (Font)Font.Clone();
            thisModel.Paragraph = (Paragraph)Paragraph.Clone();
            return thisModel;

            //todo : DeepClone - Children 이 있다면 그것도 복사해줘야 함.
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


        /// <summary>
        /// 테이블 사이즈에 맞춰서 균등비율로 테이블 셀을 업데이트해준다. 
        /// </summary>
        /// <param name="tableWidth"></param>
        /// <param name="tableHeight"></param>
        public void UpdateCellSize(int tableWidth, int tableHeight, bool isEqualRatio = true, int cellMinimumSize = 20)
        {
            if (isEqualRatio)
            {
                ColWidths = GetCelSize(tableWidth, ColCount);
                RowHeights = GetCelSize(tableHeight, RowCount);
                ColPositions = GetCellPositions(ColWidths, tableWidth);
                RowPositions = GetCellPositions(RowHeights, tableHeight);
            }
            else
            {
                int cellTotalWidth = tableWidth + ColCount - 1;
                int cellTotalHeight = tableHeight + RowCount - 1;
                //이미 설정된 값이 있다면 그것을 기준으로 업데이트한다.
                float widthRatio = (float)(cellTotalWidth) / ColWidths.Sum(x => x.Value);
                float heightRatio = (float)(cellTotalHeight) / RowHeights.Sum(x => x.Value);

                //소수점 한자리 이하로 반올림 한다.
                widthRatio = (float)Math.Round(widthRatio, 1);
                heightRatio = (float)Math.Round(heightRatio, 1);
                

                //가로 세로 비율을 업데이트 해준다.
                int colWidthSum = 0;
                for (int i = 0; i < ColWidths.Count; i++)
                {
         
                    if (i == ColWidths.Count - 1)
                    {
                        var value = cellTotalWidth - colWidthSum;
                        if (value < cellMinimumSize)
                            value = cellMinimumSize;
                        ColWidths[i] = value;
                    }
                    else
                    {
                        var value = (int)(ColWidths[i] * widthRatio);
                        if (value < cellMinimumSize)
                            value = cellMinimumSize;
                        ColWidths[i] = value;
                        colWidthSum += ColWidths[i];
                    }
                }

                int rowHeightSum = 0;
                for (int i = 0; i < RowHeights.Count; i++)
                {
                    if (i == RowHeights.Count - 1)
                    {
                        var value = cellTotalHeight - rowHeightSum;
                        if (value < cellMinimumSize)
                            value = cellMinimumSize;
                        RowHeights[i] = value;
                    }
                    else
                    {
                        var value = (int)(RowHeights[i] * heightRatio);
                        if (value < cellMinimumSize)
                            value = cellMinimumSize;
                        RowHeights[i] = value;
                        rowHeightSum += RowHeights[i];
                    }

                }
                ColPositions = GetCellPositions(ColWidths, tableWidth);
                RowPositions = GetCellPositions(RowHeights, tableHeight);
            }
           
        }
        private Dictionary<int, int> GetCelSize(int size, int count)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();
            //테두리 사이즈만큼 겹치기 때문에 변경해줌.
            int targetSize = size + (count - 1);
            int cellSize = (int)(targetSize / count);
            int remainSize = targetSize;
            for (int i = 0; i < count; i++)
            {
                if (i + 1 == count)
                    result.Add(i, remainSize);
                else
                {
                    result.Add(i, cellSize);
                    remainSize -= cellSize;
                }
            }


            return result;
        }
        private Dictionary<int, int> GetCellPositions(Dictionary<int, int> values, int lastSize)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();
            int nextPosition = 0;
            int i = 0;
            for (i = 0; i < values.Count; i++)
            {
                result.Add(i, nextPosition);

                //-1을 하는 이유는 셀을 겹치기 위함
                //그러나 이건 테두리가 1일 경우이며, 만약 2가 될경우에는 -2를 해줘야 한다..
                //그러나! 필요없을것같다. 
                //todo : [고급]테이블 테두리 기본값에 따른 로직을 넣어주는게 좋을것같긴하다.
                //문서 기본옵션값이 테두리 2라면 아래 수치를 2로 변경하게
                nextPosition += values[i] - 1;
            }

            result.Add(i, lastSize - 1);
            return result;
        }

        public (int width, int height) UpdateTableSize()
        {
            int height = RowHeights.Sum(x => x.Value) - RowCount + 1;
            RowPositions = GetCellPositions(RowHeights, height);

            int width = ColWidths.Sum(x => x.Value) - ColCount + 1;
            ColPositions = GetCellPositions(ColWidths, width);

            return (width, height);
        }

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
    }
}

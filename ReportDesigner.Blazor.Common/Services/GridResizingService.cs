using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using ReportDesigner.Blazor.Common.Utils;
using ReportDesigner.Blazor.Common.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Runtime.Intrinsics.X86;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;

namespace ReportDesigner.Blazor.Common.Services
{
    /// <summary>
    /// 워드, PPT를 보면 가로 셀 간격을 조절할경우 사이즈 추가 없이 각 셀의 영역이 변경되지만
    /// 세로 셀 간격을 조절할경우 현재 셀의 크기를 변경하고, 다른 Row는 영향이 없다. 
    /// </summary>
    public class GridResizingService
    {
        private readonly SelectionService selectedControlService;
        private readonly DesignerCSSService css;
        private readonly IJSRuntime JsRuntime;

        public GridResizingService(
            SelectionService selectedControlService, DesignerCSSService css, IJSRuntime jSRuntime)
        {
            this.selectedControlService = selectedControlService;
            this.css = css;
            this.JsRuntime = jSRuntime;

        }

        public ReportComponentModel Model
        {
            get 
            {
                if (gridModel is null)
                    throw new Exception("grid resize service model is null");
                return gridModel; 
            }
            set { gridModel = value; }
        }

        private ReportComponentModel gridModel;
        

        public enum Type
        { 
            Row,
            Col
        }

        public Type SelectLine => type;

        private Type type;

        int beforeX = 0;
        int beforeY = 0;
        int index = 0;

        public int X => x;
        public int Y => y;

        /// <summary>
        /// 이동시킬 컨트롤의 상대 X 좌표
        /// </summary>
        private int x = 0;
        private int y = 0;
        public bool IsChanged { get; set; } = false;
        public int Index => index;
        public enum Mode
        {
            None,
            Start,
            Center,
            End
        }


        private Mode mode = Mode.None;
        public void StartAction(PointerEventArgs e, int index, Type type)
        {
            Logger.Instance.Write($"index:{index} , type:{type}" , Microsoft.Extensions.Logging.LogLevel.Debug);
            this.type = type;

            this.beforeX = (int)e.ClientX;
            this.beforeY = (int)e.ClientY;

            this.index = index;

            int? count;
            if (type == Type.Col)
            {
                this.x = 0;
                count = gridModel?.TableInfo.ColPositions.Count;
            }
            else
            {
                this.y = 0;
                count = gridModel?.TableInfo.RowPositions.Count;
            }

            if (index == 0)
                mode = Mode.Start;
            else if (count == index + 1)
                mode = Mode.End;
            else
                mode = Mode.Center;
        }

        /// <summary>
        /// 그리드 라인을 실시간으로 변경시킨다.
        /// </summary>
        public void ActionMove(PointerEventArgs e)
        {
            //마우드 클릭부터 이동된 px
            int moveDistanceX = (int)e.ClientX - beforeX;
            int moveDistanceY = (int)e.ClientY - beforeY;
            Logger.Instance.Write($"move {moveDistanceX} {moveDistanceY}");
            //이동가능 영역 계산
            if (type == Type.Col)
            {
                x = Move(moveDistanceX, gridModel.TableInfo.ColPositions, index);
            }
            else
            {
                y = Move(moveDistanceY, gridModel.TableInfo.RowPositions, index);
            }
            //todo : 컨트롤 위치(마우스좌표) 표시 툴 추가필요.
            this.IsChanged = true;
        }    

        private int Move(int moveDistance, Dictionary<int,int> position, int index)
        {
            var 이동중인_컨트롤의_상대좌표 = position[index] + moveDistance;
            var 이동가능한_끝좌표 = 9999;
            var 이동가능한_시작좌표 = -9999;

            var cellMinSize = css.GridCellMinimumSize;
            if (this.mode == Mode.Start)
            {
                이동가능한_끝좌표 = position[index + 1] - cellMinSize;
            }
            else if (this.mode == Mode.End)
            {
                이동가능한_시작좌표 = position[index - 1] + cellMinSize;
            }
            else
            {
                이동가능한_끝좌표 = position[index + 1] - cellMinSize;
                이동가능한_시작좌표 = position[index - 1] + cellMinSize;
            }

            int pos = 0;
            if (이동중인_컨트롤의_상대좌표 > 이동가능한_끝좌표)
            {
                pos = 이동가능한_끝좌표;
            }
            else if (이동중인_컨트롤의_상대좌표 < 이동가능한_시작좌표)
            {
                pos = 이동가능한_시작좌표;
            }
            else
            {
                pos = 이동중인_컨트롤의_상대좌표;
            }

            return pos;
        }
        /// <summary>
        /// 정상적으로 종료할경우
        /// 현재 이동중인 좌표에 따라서 셀 간격 및 그리드 사이즈를 변경해준다. 
        /// </summary>
        public void ActionEnd()
        {

            try
            {
                if (IsChanged == false)
                    return;


                Dictionary<int, int> position = new();
                Dictionary<int, int> size = new();

                int bandsize = 0;
                int value = 0;
                bool isLastLine = false;
                int beforePosition = 0;
                if (type == Type.Col)
                {
                    position = gridModel.TableInfo.ColPositions;
                    size = gridModel.TableInfo.ColWidths;
                    value = x;
                    bandsize = this.selectedControlService.CurrentBand.Model.Width;
                    if (gridModel.TableInfo.ColPositions.Count - 1 == index)
                        isLastLine = true;
                    beforePosition = gridModel.TableInfo.ColPositions[index];
                }
                else
                {
                    position = gridModel.TableInfo.RowPositions;
                    size = gridModel.TableInfo.RowHeights;
                    value = y;
                    bandsize = this.selectedControlService.CurrentBand.Model.Height;
                    beforePosition = gridModel.TableInfo.RowPositions[index];
                    if (gridModel.TableInfo.RowPositions.Count - 1 == index)
                        isLastLine = true;
                }

                var distance = value - beforePosition;

                if (index == 0)
                {
                    if (type == Type.Col)
                    {
                        if (value + gridModel.X < 0)
                        {
                            value = 0;

                            //0에서 테이블의 시작좌표까지 구한다. 
                            distance = 0 - gridModel.X;
                        }

                        //테이블 전체 너비를 변경
                        gridModel.Width -= distance;
                        //테이블의 시작 좌표 변경
                        gridModel.X += distance;
                    }
                    else
                    {

                        if (value + gridModel.Y < 0)
                        {
                            value = 0;

                            //0에서 테이블의 시작좌표까지 구한다. 
                            distance = 0 - gridModel.Y;
                        }

                        //테이블 전체 너비를 변경
                        gridModel.Height -= distance;
                        //테이블의 시작 좌표 변경
                        gridModel.Y += distance;

                    }


                    //0번 좌표가 변경되었으므로 나머지 좌표도 전부 업데이트 해줘야 한다. 
                    for (int i = 1; i < position.Count; i++)
                    {
                        position[i] -= distance;
                    }

                    //현재 셀 너비 수정
                    size[index] -= distance;
                }
                else if(isLastLine)
                {

                    //현재 테이블의 X 좌표 + 드래그영역의 X 좌표는 밴드사이즈보다 클 수 없다. 
                    if (type == Type.Col)
                    {
                        if (gridModel.X + value > bandsize)
                        {
                            value = bandsize - gridModel.X;

                            //마우스를 놓은 좌표에서 이전 좌표까지의 거리를 구한다. 
                            distance = value - beforePosition;
                        }
                        gridModel.Width += distance;
                    }
                    else
                    {
                        if (gridModel.Y + value > bandsize)
                        {
                            value = bandsize - gridModel.Y;

                            //마우스를 놓은 좌표에서 이전 좌표까지의 거리를 구한다. 
                            distance = value - beforePosition;
                        }
                        gridModel.Height += distance;
                    }

                    //테이블 전체 너비를 변경

                    //마지막 라인 위치 변경
                    position[index] += distance;

                    //마지막 셀 너비 수정
                    size[index - 1] += distance;
           
                }
                else
                {
                    //중간을 변경하는것은 사이즈가 변하지 않는다. 
                    position[index] = value;
                    //현재라인 기준으로 좌측 셀을 사이즈 조절
                    size[index - 1] += distance;
                    //현재라인 기준으로 우측 셀의 사이즈 조절
                    size[index] -= distance;
                }



                //todo : 병합이 되어 있을 경우... 애매하네?? 아니 애매할것이 있나? 
                //한 행, 열 이 전부 병합되지 않는이상 구조는 유지된다. 
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.ToString(), Microsoft.Extensions.Logging.LogLevel.Error);
            }
            finally
            {
                ActionExit();
            }

        }

        /// <summary>
        /// 다 취소시킬 경우(정상, or Leave)
        /// </summary>
        public void ActionExit()
        {
            //변경된적이 있을 경우에만 좌표를 초기화 해준다.
            if (IsChanged)
            {
                this.x = 0;
                this.y = 0;
            }
            this.mode = Mode.None;
            this.IsChanged = false;
        }


        public async Task UpdateTableRowHeight()
        {
            Logger.Instance.Write("", LogLevel.Debug);
            var target = this.selectedControlService.CurrentSelectedModel;
            if (target == null)
            {
                Logger.Instance.Write("CurrentSelectedModel is null", Microsoft.Extensions.Logging.LogLevel.Warning);
                return;
            }


            ReportComponentModel parent;
            if (target.Type == ReportComponentModel.Control.Table || target.Type == ReportComponentModel.Control.TableCell)
            {

                if (target.Type == ReportComponentModel.Control.Table)
                {
                    parent = target;
                }
                else
                {
                    parent = target.Parent;
                }

                if (parent == null)
                {
                    Logger.Instance.Write("Parent is null", Microsoft.Extensions.Logging.LogLevel.Warning);
                    return;
                }

                //자식 오브젝트를 전부 업데이트 해줘야 한다....
                foreach (var child in parent.Children)
                {
                    //자동증가 셀일 경우(자동감소가 필요한가???
                    if (child.TableCellInfo.AutoHeightIncrease)
                    {
                        await UpdateRowHeight(child, parent);
                    }
                }

                var size = CalculateTableSize(parent.TableInfo);
                parent.Width = size.width;
                parent.Height = size.height;
            }

            async Task UpdateRowHeight(ReportComponentModel child, ReportComponentModel parent)
            {
                //현재 Row 인덱스를 가져오고
                var row = child.TableCellInfo.Row;
                //실제 TEXT 영역의 높이를 가져오고
                var height = await GetRowHeight(child);
                //todo : 예외처리좀 더 해야함.
                if (height == 0)

                {
                    return;
                }

                //if (height > Options.PaperSize.Height - Options.PaperMargin.Top - Options.PaperMargin.Bottom)
                //{
                //    Logger.Instance.Write($"용지 영역보다 큰 Row는 만들수 없습니다.{height}");
                //}
                //현재 높이와 바뀔 높이의 차이를 구한다. 
                var diff = height - parent.TableInfo.RowHeights[row];
                if (diff <= 0)
                    return;

                Logger.Instance.Write($"{row}");

                //높이를 바꾸고
                parent.TableInfo.RowHeights[row] = height;

                //parent.Height += diff;

                ////구분선의 값을 바꾼다.  
                //for (int i = row + 1; i <= parent.TableInfo.RowCount; i++)
                //{
                //    parent.TableInfo.RowPositions[i] += diff;
                //}

                //todo : 테이블 사이즈 조절하는거 한군데로 모아야 하지 않을까?
            }
        }
        private async Task<int> GetRowHeight(ReportComponentModel target)
        {
            //선택한 오브젝트에 텍스트가 없는 경우
            if (target.Text == string.Empty)
            {
                Logger.Instance.Write("Text is Empty", Microsoft.Extensions.Logging.LogLevel.Warning);
                return 0;
            }
            //선택된 오브젝트의 UID로 클라이언트의 사이즈를 가져온다.
            var value = await JsRuntime.InvokeAsync<ComponentTextSize>("GetInnerTextHeight", target.Uid);
            if (value == null)
            {
                Logger.Instance.Write("value is null", Microsoft.Extensions.Logging.LogLevel.Warning);
                return 0;
            }
            var inner = (int)value.inner;
            return (css.GlobalPadding * 2) + inner;
        }

        public void UpdateTable(int width, int height)
        {
            var control = this.selectedControlService.LastSelectModel;
            //일반 컨트롤의 경우 모델사이즈를 변경하고, 리프레시를 해주면 반영되지만.
            //테이블의 경우 각 셀의 사이즈에 따라서 외부 Tr 의 사이즈가 변경된다..
            if (control.Type != ReportComponentModel.Control.Table)
                return;

            UpdateCellSize(control.TableInfo, width, height, false, css.GridCellMinimumSize);

        }


        /// <summary>
        /// 테이블 사이즈에 맞춰서 균등비율로 테이블 셀을 업데이트해준다. 
        /// </summary>
        /// <param name="tableWidth"></param>
        /// <param name="tableHeight"></param>
        public void UpdateCellSize(TableInfo table, int tableWidth, int tableHeight, bool isEqualRatio = true, int cellMinimumSize = 20)
        {
            if (isEqualRatio)
            {
                table.ColWidths = GetCelSize(tableWidth, table.ColCount);
                table.RowHeights = GetCelSize(tableHeight, table.RowCount);
                table.ColPositions = GetCellPositions(table.ColWidths, tableWidth);
                table.RowPositions = GetCellPositions(table.RowHeights, tableHeight);
            }
            else
            {
                int cellTotalWidth = tableWidth + table.ColCount - 1;
                int cellTotalHeight = tableHeight + table.RowCount - 1;
                //이미 설정된 값이 있다면 그것을 기준으로 업데이트한다.
                float widthRatio = (float)(cellTotalWidth) / table.ColWidths.Sum(x => x.Value);
                float heightRatio = (float)(cellTotalHeight) / table.RowHeights.Sum(x => x.Value);

                //소수점 한자리 이하로 반올림 한다.
                widthRatio = (float)Math.Round(widthRatio, 1);
                heightRatio = (float)Math.Round(heightRatio, 1);


                //가로 세로 비율을 업데이트 해준다.
                int colWidthSum = 0;
                for (int i = 0; i < table.ColWidths.Count; i++)
                {

                    if (i == table.ColWidths.Count - 1)
                    {
                        var value = cellTotalWidth - colWidthSum;
                        if (value < cellMinimumSize)
                            value = cellMinimumSize;
                        table.ColWidths[i] = value;
                    }
                    else
                    {
                        var value = (int)(table.ColWidths[i] * widthRatio);
                        if (value < cellMinimumSize)
                            value = cellMinimumSize;
                        table.ColWidths[i] = value;
                        colWidthSum += table.ColWidths[i];
                    }
                }

                int rowHeightSum = 0;
                for (int i = 0; i < table.RowHeights.Count; i++)
                {
                    if (i == table.RowHeights.Count - 1)
                    {
                        var value = cellTotalHeight - rowHeightSum;
                        if (value < cellMinimumSize)
                            value = cellMinimumSize;
                        table.RowHeights[i] = value;
                    }
                    else
                    {
                        var value = (int)(table.RowHeights[i] * heightRatio);
                        if (value < cellMinimumSize)
                            value = cellMinimumSize;
                        table.RowHeights[i] = value;
                        rowHeightSum += table.RowHeights[i];
                    }

                }
                table.ColPositions = GetCellPositions(table.ColWidths, tableWidth);
                table.RowPositions = GetCellPositions(table.RowHeights, tableHeight);
            }

            Dictionary<int, int> GetCelSize(int size, int count)
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

        public (int width, int height) CalculateTableSize(TableInfo info)
        {
            int height = info.RowHeights.Sum(x => x.Value) - info.RowCount + 1;
            info.RowPositions = GetCellPositions(info.RowHeights, height);

            int width = info.ColWidths.Sum(x => x.Value) - info.ColCount + 1;
            info.ColPositions = GetCellPositions(info.ColWidths, width);

            return (width, height);
        }
    }

    //todo : 테이블 스냅포인트 계산 잘 안됨.

}

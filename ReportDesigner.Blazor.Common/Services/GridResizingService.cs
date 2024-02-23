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

        public GridResizingService(
            SelectionService selectedControlService, DesignerCSSService css)
        {
            this.selectedControlService = selectedControlService;
            this.css = css;
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
            Logger.Instance.Write($"index:{index} , type:{type}");
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

        //private void MoveHorizontal(int moveDistanceX)
        //{
        //    var 이동중인_컨트롤의_상대좌표 = gridModel.TableInfo.ColPositions[index] + moveDistanceX;
        //    var 이동가능한_우측_최대좌표 = 9999;
        //    var 이동가능한_좌측_최대좌표 = -9999;

        //    var cellMinSize = css.GridCellMinimumSize;
        //    if (this.mode == Mode.Start)
        //    {
        //        이동가능한_우측_최대좌표 = gridModel.TableInfo.ColPositions[index + 1] - cellMinSize;
        //    }
        //    else if (this.mode == Mode.End)
        //    {
        //        이동가능한_좌측_최대좌표 = gridModel.TableInfo.ColPositions[index - 1] + cellMinSize;
        //    }
        //    else
        //    {
        //        이동가능한_우측_최대좌표 = gridModel.TableInfo.ColPositions[index + 1] - cellMinSize;
        //        이동가능한_좌측_최대좌표 = gridModel.TableInfo.ColPositions[index - 1] + cellMinSize;                
        //    }

        //    if (이동중인_컨트롤의_상대좌표 > 이동가능한_우측_최대좌표)
        //    {
        //        x = 이동가능한_우측_최대좌표;
        //    }
        //    else if (이동중인_컨트롤의_상대좌표 < 이동가능한_좌측_최대좌표)
        //    {
        //        x = 이동가능한_좌측_최대좌표;
        //    }
        //    else
        //    {
        //        x = 이동중인_컨트롤의_상대좌표;
        //    }
        //    Logger.Instance.Write($"이동중인_컨트롤의_상대좌표 {이동중인_컨트롤의_상대좌표} 실제 X {x} ");
        //}

        //private void MoveVertical(int moveDistanceY)
        //{

        //    if (this.mode == Mode.Start)
        //    {
        //        이동가능한_끝좌표 = gridModel.TableInfo.ColPositions[index + 1] - cellMinSize;
        //    }
        //    else if (this.mode == Mode.End)
        //    {
        //        이동가능한_시작좌표 = gridModel.TableInfo.ColPositions[index - 1] + cellMinSize;
        //    }
        //    else
        //    {
        //        이동가능한_끝좌표 = gridModel.TableInfo.ColPositions[index + 1] - cellMinSize;
        //        이동가능한_시작좌표 = gridModel.TableInfo.ColPositions[index - 1] + cellMinSize;
        //    }
        //}

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
                Logger.Instance.Write(ex.ToString());
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
    }
}

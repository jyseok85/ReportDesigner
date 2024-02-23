using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using ReportDesigner.Blazor.Common.Data.Model;
using ReportDesigner.Blazor.Common.Utils;

namespace ReportDesigner.Blazor.Common.Services
{
    public class ResizingService
    {
        private readonly SelectionService selectedControlService;
        private readonly DesignerCSSService? css;
        private readonly IJSRuntime JsRuntime;

        public ResizingService(
            SelectionService selectedControlService, DesignerCSSService? css, IJSRuntime jSRuntime)
        {
            this.selectedControlService = selectedControlService;
            this.css = css;
            this.JsRuntime = jSRuntime;
        }

        public string Id { get; set; } = string.Empty;
        //여기채우기
        public int Width { get; set; } = 100;

        public int Height { get; set; } = 30;

        public int X { get; set; } = 0;

        public int Y { get; set; } = 0;

        public bool IsChanged { get; set; } = false;

        private string grabbedControlPosition = string.Empty;

        int beforeX = 0;
        int beforeY = 0;
        int beforeRight = 0;
        int beforeBottom = 0;

        int beforeWidth = 0;
        int beforeHeight = 0;


        public void ActionStart(PointerEventArgs e, string type, string id)
        {
            this.Id = id;
            ActionStart(e.ClientX, e.ClientY, type);

        }
        private void ActionStart(double clientX, double clientY, string type)
        {
            this.beforeX = (int)clientX;
            this.beforeY = (int)clientY;

            this.beforeRight = (int)clientX + Width;
            this.beforeBottom = (int)clientY + Height;
            this.beforeWidth = Width;
            this.beforeHeight = Height;
            this.grabbedControlPosition = type;
        }

        /// <summary>
        /// ResizeArea를 실시간으로 변경한다. 
        /// </summary>
        /// <param name="e"></param>
        public void ActionMove(PointerEventArgs e)
        {
            ActionMove(e.ClientX, e.ClientY);
        }
        public void ActionMove(double clientX, double clientY)
        {
            int moveDistanceX = (int)clientX - beforeX;
            int moveDistanceY = (int)clientY - beforeY;

            foreach (char c in this.grabbedControlPosition)
            {
                switch (c)
                {
                    case 'w': MoveLeftControl(); break;
                    case 'e': MoveRightControl(); break;
                    case 's': MoveBottomControl(); break;
                    case 'n': MoveTopControl(); break;
                }
            }

            this.IsChanged = true;
            void MoveLeftControl()
            {
                Logger.Instance.Write($"MoveTopControl {beforeX},{moveDistanceX},{beforeRight}");
                //좌측 조절일때는 우측 영역을 벗어날수 없다. 
                if (beforeX + moveDistanceX >= beforeRight)
                {

                    this.X = beforeRight - beforeX - 1;
                    this.Width = 0;

                    //X = moveDitanceX;

                    //Width = beforeRight - (beforeX + moveDitanceX);
                    Logger.Instance.Write($"MoveTopControl 1");
                }
                else
                {
                    this.X = moveDistanceX;
                    this.Width = beforeWidth - X;
                    Logger.Instance.Write($"MoveTopControl 2 {Width}");
                }
            }
            void MoveTopControl()
            {
                //Logger.Instance.Write($"MoveTopControl {beforeY},{moveDitanceY},{beforeBottom}");
                if (beforeY + moveDistanceY >= beforeBottom)
                {
                    this.Y = beforeBottom - beforeY - 1;
                    this.Height = 0;
                }
                else
                {
                    this.Y = moveDistanceY;
                    this.Height = beforeHeight - Y;
                }
            }
            void MoveRightControl()
            {
                if (moveDistanceX + beforeWidth < X)
                    this.Width = 0;
                else
                    this.Width = beforeWidth + moveDistanceX;
                Logger.Instance.Write($"Width {Width}");
            }
            void MoveBottomControl()
            {
                if (moveDistanceY + beforeHeight < Y)
                    this.Height = 0;
                else
                    this.Height = beforeHeight + moveDistanceY;
            }

        }





        public async Task ActionEnd()
        {
            //마지막 선택된 컨트롤의 사이즈를 변경해준다.
            if (IsChanged)
            {
               // UpdateLastSelectedControlSize();
               // await selectedControlService.UpdateInnerTextControlScale();


                var size = GetModifiedControlSize();
                await selectedControlService.UpdateInnerTextControlScale(size.Item1);
                this.Width = size.Item1;
                this.Height = size.Item2;
                var control = this.selectedControlService.LastSelectModel;
                control.Width = this.Width;
                control.Height = this.Height;
                UpdateTable(this.Width, this.Height);
                SetBandHeight();

            }
            ActionExit();
        }

        private void SetBandHeight()
        {
            var control = this.selectedControlService.LastSelectModel;
            var band = this.selectedControlService.CurrentBand.Model;
            //밴드 Bottom 영역 이상으로 커진다면 밴드를 늘려버린다.
            if (control.Bottom > band.Height)
                band.Height = control.Bottom;
        }

        private void UpdateTable(int width, int height)
        {
            var control = this.selectedControlService.LastSelectModel;
            //일반 컨트롤의 경우 모델사이즈를 변경하고, 리프레시를 해주면 반영되지만.
            //테이블의 경우 각 셀의 사이즈에 따라서 외부 Tr 의 사이즈가 변경된다..
            if (control.Type == ReportComponentModel.Control.Table)
            {
                control.TableInfo.UpdateCellSize(width, height);
            }
        }
        private (int, int) GetModifiedControlSize()
        {
            if (this.selectedControlService is null || this.css is null)
                return (0, 0);

            if (this.selectedControlService.CurrentBand is null)
                return (0, 0);

            var currentBand = this.selectedControlService.CurrentBand.Model;
            if (currentBand is null)
                return (0, 0); 

            var lastSelectedModel = this.selectedControlService.LastSelectModel;
            lastSelectedModel.X += X;
            lastSelectedModel.Y += Y;

            lastSelectedModel.AbsoluteOffsetY += Y;
            int width = this.Width;
            int height = this.Height;

            if (lastSelectedModel.X < 0)
            {
                width += lastSelectedModel.X;
                lastSelectedModel.X = 0;
                lastSelectedModel.AbsoluteOffsetX = currentBand.AbsoluteOffsetX;
            }
            else
                lastSelectedModel.AbsoluteOffsetX = currentBand.AbsoluteOffsetX + lastSelectedModel.X;


            if (lastSelectedModel.Y < 0)
            {
                height += lastSelectedModel.Y;
                lastSelectedModel.Y = 0;
                lastSelectedModel.AbsoluteOffsetY = currentBand.AbsoluteOffsetY;
            }
            else
            {
                lastSelectedModel.AbsoluteOffsetY = currentBand.AbsoluteOffsetY + lastSelectedModel.Y;

            }

            //오른쪽 밴드 이후 영역으로 나가는지 체크
            if (X >= 0)
            {
                //용지 기준으로 변경하는 사이즈의 절대 좌표
                var targetX = width + lastSelectedModel.AbsoluteOffsetX; //이것대신 왼쪽 마진을 더해도 된다. 
                var bandMaxAbsoluteX = currentBand.Width + currentBand.AbsoluteOffsetX; //이것대신 왼쪽 마진을 더해도 된다. 

                if (targetX > bandMaxAbsoluteX)
                {
                    //초과된 범위만큼 가로 사이즈를 조절한다. 
                    int diff = targetX - bandMaxAbsoluteX;
                    width = width - diff;
                }

            }

            string msg = $"X:{lastSelectedModel.X}, Width:{width}";
            Logger.Instance.Write(msg);

            //컨트롤의 최소사이즈는 패딩사이즈(이 값일때 텍스트 표시불가)
            int minimumWidth = this.css.GlobalPadding * 2;
            int minimumHeight = this.css.GlobalPadding * 2;

            if (lastSelectedModel.Type == ReportComponentModel.Control.Table)
            {
                minimumWidth = (minimumWidth * lastSelectedModel.TableInfo.ColCount) + 1;
                minimumHeight = (minimumHeight * lastSelectedModel.TableInfo.RowCount) + 1;
            }

            //컨트롤의 최소사이즈 지정
            if (minimumWidth > width)
                width = minimumWidth;

            if (minimumHeight > height)
                height = minimumHeight;

            return (width, height);
        }


        /// <summary>
        /// ResizeArea 사이즈에 맞춰서 선택된 컨트롤의 사이즈를 변경시켜준다. 
        /// </summary>
        private void UpdateLastSelectedControlSize()
        {
            if (this.selectedControlService is null || this.css is null)
                return;

            if (this.selectedControlService.CurrentBand is null)
                return;

            var currentBand = this.selectedControlService.CurrentBand.Model;
            if (currentBand is null)
                return;

            var lastSelectedModel = this.selectedControlService.LastSelectModel;
            lastSelectedModel.X += X;
            lastSelectedModel.Y += Y;

            lastSelectedModel.AbsoluteOffsetY += Y;
            int width = this.Width;
            int height = this.Height;

            if (lastSelectedModel.X < 0)
            {
                width += lastSelectedModel.X;
                lastSelectedModel.X = 0;
                lastSelectedModel.AbsoluteOffsetX = currentBand.AbsoluteOffsetX;
            }
            else
                lastSelectedModel.AbsoluteOffsetX = currentBand.AbsoluteOffsetX + lastSelectedModel.X;


            if (lastSelectedModel.Y < 0)
            {
                height += lastSelectedModel.Y;
                lastSelectedModel.Y = 0;
                lastSelectedModel.AbsoluteOffsetY = currentBand.AbsoluteOffsetY;
            }
            else
            {
                lastSelectedModel.AbsoluteOffsetY = currentBand.AbsoluteOffsetY + lastSelectedModel.Y;

            }

            //오른쪽 밴드 이후 영역으로 나가는지 체크
            if (X >= 0)
            {
                //용지 기준으로 변경하는 사이즈의 절대 좌표
                var targetX = width + lastSelectedModel.AbsoluteOffsetX; //이것대신 왼쪽 마진을 더해도 된다. 
                var bandMaxAbsoluteX = currentBand.Width + currentBand.AbsoluteOffsetX; //이것대신 왼쪽 마진을 더해도 된다. 

                if (targetX > bandMaxAbsoluteX)
                {
                    //초과된 범위만큼 가로 사이즈를 조절한다. 
                    int diff = targetX - bandMaxAbsoluteX;
                    width = width - diff;
                }

            }

            string msg = $"X:{lastSelectedModel.X}, Width:{width}";
            Logger.Instance.Write(msg);

            //컨트롤의 최소사이즈는 패딩사이즈(이 값일때 텍스트 표시불가)
            int minimumWidth = this.css.GlobalPadding * 2;
            int minimumHeight = this.css.GlobalPadding * 2;

            if (lastSelectedModel.Type == ReportComponentModel.Control.Table)
            {
                minimumWidth = (minimumWidth * lastSelectedModel.TableInfo.ColCount) + 1;
                minimumHeight = (minimumHeight * lastSelectedModel.TableInfo.RowCount) + 1;
            }

            //컨트롤의 최소사이즈 지정
            if (minimumWidth > width)
                width = minimumWidth;

            if (minimumHeight > height)
                height = minimumHeight;


            //일반 컨트롤의 경우 모델사이즈를 변경하고, 리프레시를 해주면 반영되지만.
            //테이블의 경우 각 셀의 사이즈에 따라서 외부 Tr 의 사이즈가 변경된다..
            if (lastSelectedModel.Type == ReportComponentModel.Control.Table)
            {
                lastSelectedModel.TableInfo.UpdateCellSize(width, height);
            }



            this.Width = lastSelectedModel.Width = width;
            this.Height = lastSelectedModel.Height = height;

            //밴드 Bottom 영역 이상으로 커진다면 밴드를 늘려버린다.
            if (lastSelectedModel.Bottom > currentBand.Height)
                currentBand.Height = lastSelectedModel.Bottom;
        }

       

        public void ActionExit()
        {
  
            this.IsChanged = false;
            this.X = 0;
            this.Y = 0;
        }
        public void UpdateSize(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            ActionExit();
        }

    }

}

using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportDesigner.Blazor.Common.Services
{
    public class ControlModificationServcie
    {
        public string Id { get; set; } = string.Empty;
        //여기채우기
        public int Width { get; set; } = 100;

        public int Height { get; set; } = 30;

        public int X { get; set; } = 0;

        public int Y { get; set; } = 0;

        public bool IsChanged { get; set; } = false;

        private bool grapedResizeControl = false;
        public bool GrapedResizeControl { get { return this.grapedResizeControl; } set { this.grapedResizeControl = value; } }
        private string grabedControlPosition = string.Empty;

        int beforeX = 0;
        int beforeY = 0;
        int beforeRight = 0;
        int beforeBottom = 0;

        int beforeWidth = 0;
        int beforeHeight = 0;
        public void ActiveResizeComponent(int width, int height)
        {
            X = 0;
            Y = 0;
            Width = (int)width;
            Height = (int)height;
            IsChanged = false;
        }

        public void ActionStart(PointerEventArgs e, string type, string id)
        {
            this.Id = id;
            ActionStart(e.ClientX, e.ClientY, type);

        }
        private void ActionStart(double clientX, double clientY, string type)
        {
            beforeX = (int)clientX;
            beforeY = (int)clientY;

            beforeRight = (int)clientX + Width;
            beforeBottom = (int)clientY + Height;
            beforeWidth = Width;
            beforeHeight = Height;

            grapedResizeControl = true;
            grabedControlPosition = type;

            //Console.WriteLine($"Resize ActionStart {beforeY},{beforeBottom}");

        }
        public void ActionMove(PointerEventArgs e)
        {
            ActionMove(e.ClientX, e.ClientY);
        }
        public void ActionMove(double clientX, double clinetY)
        {
            if (grapedResizeControl == false)
                return;

            int moveDitanceX = (int)clientX - beforeX;
            int moveDitanceY = (int)clinetY - beforeY;

            foreach (char c in this.grabedControlPosition)
            {
                switch (c)
                {
                    case 'w': MoveLeftControl(); break;
                    case 'e': MoveRightControl(); break;
                    case 's': MoveBottomControl(); break;
                    case 'n': MoveTopControl(); break;
                }
            }

            IsChanged = true;
            void MoveLeftControl()
            {
                Console.WriteLine($"MoveTopControl {beforeX},{moveDitanceX},{beforeRight}");
                //좌측 조절일때는 우측 영역을 벗어날수 없다. 
                if (beforeX + moveDitanceX >= beforeRight)
                {

                    X = beforeRight - beforeX - 1;
                    Width = 0;

                    //X = moveDitanceX;

                    //Width = beforeRight - (beforeX + moveDitanceX);
                Console.WriteLine($"MoveTopControl 1");
                }
                else
                {
                    X = moveDitanceX;
                    Width = beforeWidth - X;
                Console.WriteLine($"MoveTopControl 2 {Width}");
                }
            }
            void MoveTopControl()
            {
                //Console.WriteLine($"MoveTopControl {beforeY},{moveDitanceY},{beforeBottom}");
                if (beforeY + moveDitanceY >= beforeBottom)
                {
                    Y = beforeBottom - beforeY - 1;
                    Height = 0;
                }
                else
                {
                    Y = moveDitanceY;
                    Height = beforeHeight - Y;
                }
            }
            void MoveRightControl()
            {
                if (moveDitanceX + beforeWidth < X)
                    Width = 0;
                else
                    Width = beforeWidth + moveDitanceX;
            }
            void MoveBottomControl()
            {
                if (moveDitanceY + beforeHeight < Y)
                    Height = 0;
                else
                    Height = beforeHeight + moveDitanceY;
            }

        }

        

        public void ActionEnd()
        {
            grapedResizeControl = false;
            IsChanged = false;
            X = 0;
            Y = 0;
        }
    }
}

using Microsoft.Extensions.ObjectPool;
using ReportDesigner.Blazor.Common.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportDesigner.Blazor.Common.Services
{
    public class DragAndDropService
    {
        public string Uid { get; set; } = string.Empty;
        public double PosX { get; set; }
        public double PosY { get; set; }
        public int Right => (int)PosX + Width;

        public int Bottom => (int)PosY + Height;

        public int Width { get; set; } = 100;

        public int Height { get; set; } = 30;


        public double AbsoluteX { get; set; } = 0;

        public double AbsoluteY { get; set; } = 0;

        private double diffX;
        private double diffY;

        public double MouseX { get; set; }
        public double MouseY { get; set; }

        /// <summary>
        /// 드래그 오브젝트가 보일지 말지여부
        /// </summary>
        public bool Hidden { get; set; } = true;

        public void StartDrag(string Uid, int objectPosX, int objectPosY, int objectWidth, int objectHeight, double mouseClientX, double mouseClientY)
        {
            this.Uid = Uid;
            PosX = objectPosX;
            PosY = objectPosY;
            Width = objectWidth;
            Height = objectHeight;
            diffX = mouseClientX - PosX;
            diffY = mouseClientY - PosY;
            MouseX = mouseClientX;
            MouseY = mouseClientY;
            //Hidden = false;
        }
        public void StartDrag(ReportComponentModel model, double mouseClientX, double mouseClientY)
        {
            this.Uid = Uid;
            PosX = model.X;
            PosY = model.Y;
            Width = model.Width;
            Height = model.Width;
            diffX = mouseClientX - PosX;
            diffY = mouseClientY - PosY;
            MouseX = mouseClientX;
            MouseY = mouseClientY;
            
        }

        public void UpdatePos(double pointerX, double pointerY)
        {
            diffX = pointerX - PosX;
            diffY = pointerY - PosY;
            MouseX = pointerX;
            MouseY = pointerY;
        }
        public void Move(double pointerClientX, double pointerClientY)
        {
            PosX = pointerClientX - diffX;
            PosY = pointerClientY - diffY;

            Console.WriteLine($"{PosX} {PosY}");

        }

        public void End()
        {
            this.Hidden = true;
        }
    }
}

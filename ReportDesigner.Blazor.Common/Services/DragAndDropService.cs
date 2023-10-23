using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportDesigner.Blazor.Common.Services
{
    internal class DragAndDropService
    {
        public string Uid { get; set; } = string.Empty;
        public int PosX { get; set; }
        public int PosY { get; set; }
        public void StartDrag(string Uid, int posX, int posY)
        {
            this.Uid = Uid;
            this.PosX = posX;
            this.PosY = posY;
        }
    }
}

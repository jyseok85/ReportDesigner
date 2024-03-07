using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Radzen.Blazor;
using ReportDesigner.Blazor.Common.Data.BaseClass;
using ReportDesigner.Blazor.Common.Data.EtcComponents;
using ReportDesigner.Blazor.Common.Data.Model;
using ReportDesigner.Blazor.Common.UI.ReportControls.Controls;
using ReportDesigner.Blazor.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ReportDesigner.Blazor.Common.Data.BaseClass.BandBase;

namespace ReportDesigner.Blazor.Common.Services
{
    public class CreationService
    {
        public readonly SelectionService SelectionService;
        public readonly DesignerCSSService CSS;
        private readonly GridResizingService gridResizingService;
        private readonly ResizingService resizingService;



        public CreationService(SelectionService selectedControlService, DesignerCSSService css, GridResizingService gridResizingService, ResizingService resizingService)
        {
            this.SelectionService = selectedControlService;
            this.CSS = css;
            this.gridResizingService = gridResizingService;
            this.resizingService = resizingService;
        }
        public int Width { get; set; } = 100;

        public int Height { get; set; } = 30;

        public int X { get; set; } = -1;

        public int Y { get; set; } = -1;

        public bool Hidden { get; set; } = true;

        private int diffX;
        private int diffY;

        public int ClientX { get; set; }
        public int ClientY { get; set; }


        public void ActionStart(PointerEventArgs e)
        {
            ActionStart(e.OffsetX, e.OffsetY, e.ClientX, e.ClientY);
        }

        public void ActionStart(double offsetX, double offsetY, double clientX, double clientY)
        {
            Hidden = false;
            X = (int)offsetX;
            Y = (int)offsetY;
            Width = 0;
            Height = 0;

            ClientX = (int)clientX;
            ClientY = (int)clientY;

            diffX = (int)clientX - X;
            diffY = (int)clientY - Y;
        }
        public void ActionMove(PointerEventArgs e)
        {
            ActionMove(e.ClientX, e.ClientY);
        }
        public void ActionMove(double clientX, double clinetY)
        {
            //계산을 offset 이 아닌 client로 하는 이유는
            //offset 계산할경우 마우스를 역방향으로 이동해서 현재 생성하는 컨트롤 안으로 이동할때 offset 이 달라지기 때문에
            //전체 페이지에대한 client 좌표를 사용해서 계산한다. 
            if (Hidden)
                return;

            double x = clientX - diffX;
            double y = clinetY - diffY;

            Width = (int)(x - X) - 1;
            Height = (int)(y - Y) - 1;
            Logger.Instance.Write($"ActionMove {x} {X} {Width},{Height}");
        }
        public void ActionExit()
        {
            Width = 0;
            Height = 0;
            Hidden = true;
        }
        public void ActionEnd()
        {
            CreateControl();
            ActionExit();
        }

        public void CreateControl(ReportComponentModel.Control type = ReportComponentModel.Control.Label, object result = null)
        {
            //최소 사이즈 이상 드래그 된 경우만 진행한다. ?? 아니면 작게 그리면 최소사이즈만큼 그려준다?
            int minimumWidth = CSS.GlobalPadding * 2;
            int minimumHeight = CSS.GlobalPadding * 2;

            if (this.Width <= minimumWidth || this.Height <= minimumHeight)
                return;

            //새로 생성하는 컨트롤에 TabIndex를 할당해서 키보드 이벤트를 받도록 한다. 
            var control = new ControlBase(X, Y, Width, Height);
            control.Model.Type = type;

            if (type == ReportComponentModel.Control.Table && result != null)
            {
                var dic = ((Dictionary<string, int>)result);
                int rowCount = dic["row"];
                int colCount = dic["col"];
                int value = dic["type"];

                CreateTableControl(control, rowCount, colCount, value);
            }
            Logger.Instance.Write($"{control.Model.Type} X:{control.Model.X} Y:{control.Model.Y} W:{control.Model.Width} H:{control.Model.Height}", Microsoft.Extensions.Logging.LogLevel.Trace);
            SelectionService.CurrentBand?.AddControl(control);
            SelectionService.SelectControl(false, control.Model, SelectionService.CurrentBand);
            this.resizingService.UpdateSize(control.Model.Width, control.Model.Height);

        }

        public void PasteControl(ReportComponentModel model, BandBase band, Location location = null)
        {
            if (band is not null)
            {
                var control = new ControlBase();
                control.Model = model;

                band.AddControl(control, location);

                //붙여넣기 한 컨트롤을 선택해 주어야 한다.
                SelectionService.SelectControl(false, model, band);

                SelectionService.CopiedModel = null;
            }
        }

        private void CreateTableControl(ControlBase control, int rowCount, int colCount, int value)
        {
            if (value > 1) //todo : 이렇게 인덱스로 해버리면 나중에 값을 이해하기 어렵다. 문자열로 바꾸자.
            {
                control.Model.X = 0;
                control.Model.Width = SelectionService.CurrentBand.Model.Width;
            }
            if (value > 2)
            {
                control.Model.Y = 0;
                control.Model.Height = SelectionService.CurrentBand.Model.Height;
            }

            control.Model.TableInfo = new TableInfo();
            control.Model.TableInfo.RowCount = rowCount;
            control.Model.TableInfo.ColCount = colCount;

            gridResizingService.UpdateCellSize(control.Model.TableInfo, this.Width, this.Height);

            control.Model.Border = new Border(0, 0, 0, 0);          

            int targetHeight = this.Height - (rowCount - 1);



            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    var model = new ReportComponentModel();
                    model.TableCellInfo = new TableCellInfo();
                    model.TableCellInfo.Col = c;
                    model.TableCellInfo.Row = r;
                    model.Type = ReportComponentModel.Control.TableCell;
                    model.ParentUid = control.Model.Uid;
                    model.Parent = control.Model;
                    control.Model.Children.Add(model);
                }
            }
        }

    }
}

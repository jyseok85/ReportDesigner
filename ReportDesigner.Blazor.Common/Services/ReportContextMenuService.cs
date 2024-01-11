using Append.Blazor.Clipboard;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using ReportDesigner.Blazor.Common.Data.BaseClass;
using ReportDesigner.Blazor.Common.Data.EtcComponents;

namespace ReportDesigner.Blazor.Common.Services
{
    public class ReportContextMenuService
    {
        [Inject]
        public required SelectedControlService SelectedControlService { get; set; }

        [Inject]
        public required ContextMenuService ContextMenuService { get; set; }

        [Inject]
        public required ControlCreationService ControlCreationService { get; set; }

        [Inject]
        public required DesignerOptionService Options { get; set; }

        [Inject]
        public required IClipboardService ClipboardService { get; set; }

        int lastMouseX = 0;
        int lastMouseY = 0;
        public void ShowContextMenuWithItems(MouseEventArgs args, Point reportLocation)
        {
            lastMouseX = (int)args.ClientX - Options.PaperMargin.Left - (int)reportLocation.X;
            lastMouseY = (int)args.ClientY - Options.PaperMargin.Top - (int)reportLocation.Y;
            //마우스위치는 
            Console.WriteLine($"{args.ClientX} {args.ClientY}");
            Console.WriteLine($"{lastMouseX} {lastMouseY}");
            if (SelectedControlService == null)
            {
                Console.WriteLine("선택된 컨트롤이 없습니다.");
                return;
            }
            var type = SelectedControlService.CurrentSelectedModel.Type;
            Console.WriteLine($"ShowContextMenuWithItems {type}");
            var menuList = new List<ContextMenuItem>();

            int index = 1;

            switch (type)
            {
                case Data.Model.ReportComponentModel.Control.Table:
                case Data.Model.ReportComponentModel.Control.Label:
                    menuList.Add(CreateMenu("Lock", "home"));
                    menuList.Add(CreateMenu("Bring to front", "home"));
                    menuList.Add(CreateMenu("Send to back", "home"));
                    menuList.Add(CreateMenu("Duplicate", "home"));
                    menuList.Add(CreateMenu("Delete", "home")); //휴지통모양
                    menuList.Add(CreateMenu("Copy", "copy"));
                    break;
                case Data.Model.ReportComponentModel.Control.Report:
                case Data.Model.ReportComponentModel.Control.Layer:
                    break;
                case Data.Model.ReportComponentModel.Control.Band:
                    if (SelectedControlService.CopiedModel is not null)
                        menuList.Add(CreateMenu("Paste", "paste"));
                    break;

            }

            if (type == Data.Model.ReportComponentModel.Control.Label)
            {
                menuList.Add(CreateMenu("Copy Content", "info"));
                //   menuList.Add(CreateMenu("Edit", "home"));
            }

            menuList.Add(CreateMenu("Info", "home"));//속성창 열기


            ContextMenuService.Open(args, menuList, OnMenuItemClick);

            ContextMenuItem CreateMenu(string text, string icon)
            {
                var menu = new ContextMenuItem();
                menu.Text = text;
                menu.Icon = icon;
                menu.Value = index++;
                return menu;
            }
        }

        public void PasteControl(bool useLastMousePos)
        {
            if (SelectedControlService.CopiedModel == null)
            {
                Console.WriteLine("복사된 컨트롤이 없습니다.");
                return;
            }
            var band = SelectedControlService.RazorComponent as BandBase;

            Location loc = null;
            if(useLastMousePos == true)
            {
                loc = new Location(lastMouseX, lastMouseY);
            }

            ControlCreationService.PasteControl(SelectedControlService.CopiedModel, band, loc);
            SelectedControlService.CopiedModel = null;
            Options.RefreshBody();
        }
        public async void OnMenuItemClick(MenuItemEventArgs args)
        {
            var action = args.Text.ToLower();
            if (action == "copy")
            {
                SelectedControlService.CopyControl();
            }
            else if (action == "copy content")
            {
                var text = SelectedControlService.CurrentSelectedModel.Text;
                //클립보드를 복사하기 전에는 Document 에 포커스가 있어야 하기 때문에 컨텍스트 메뉴 팝업을 먼저 닫아 버린다. 
                ContextMenuService.Close();
                await ClipboardService.CopyTextToClipboardAsync(text);
                Console.WriteLine($"Clipboard Copied : {text}");
            }
            else if (action == "paste") //부모밴드가 왜 Null?
            {
                PasteControl(true);
            }
            else if(action == "send to back")
            {
                //현재 선택한 컨트롤의 z-index를 가져온다. 
                int zIndex = SelectedControlService.CurrentSelectedModel.ZIndex;

                var controls = SelectedControlService.CurrentBand.controlBases;

                //현재 값보다 작은 컨트롤만 찾는다.
                var targetControls = controls.Where(x => x.Model.ZIndex < zIndex);

                //검색된 값중 가장 큰값을 찾는다.
                var target = targetControls.OrderByDescending(x => x.Model.ZIndex).First();
                //var target = targetList.Max(x => x.Model.ZIndex);

                if(target is not null)
                {
                    //교환하기
                    int old = target.Model.ZIndex;
                    target.Model.ZIndex = zIndex;
                    SelectedControlService.CurrentSelectedModel.ZIndex = old;

                    //상태변경을 해줘야한다. 
                    Options.RefreshBody();
                }
                else
                {
                    Console.WriteLine("현재 컨트롤이 제일 아래에 있습니다.");
                }
            }
            else if(action == "bring to front")
            {
                //현재 선택한 컨트롤의 z-index를 가져온다. 
                int zIndex = SelectedControlService.CurrentSelectedModel.ZIndex;

                var controls = SelectedControlService.CurrentBand.controlBases;

                //현재 큰 컨트롤만 찾는다. 
                var targetControls = controls.Where(x => x.Model.ZIndex > zIndex);

                //검색된 값중 가장 작은값을 찾는다.
                var target = targetControls.OrderBy(x => x.Model.ZIndex).First();

                if (target is not null)
                {
                    //교환하기
                    int old = target.Model.ZIndex;
                    target.Model.ZIndex = zIndex;
                    SelectedControlService.CurrentSelectedModel.ZIndex = old;

                    //상태변경을 해줘야한다. 
                    Options.RefreshBody();
                }
                else
                {
                    Console.WriteLine("현재 컨트롤이 제일 위에 있습니다.");
                }
            }
            ContextMenuService.Close();
        }


        
    }
}

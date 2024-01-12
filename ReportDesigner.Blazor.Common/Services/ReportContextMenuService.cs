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
                    menuList.Add(CreateMenu("Remove", "home")); //휴지통모양
                    menuList.Add(CreateMenu("Copy", "copy"));
                    menuList.Add(CreateMenu("Cut", "cut"));
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

        public void DuplicateControl()
        {
            Console.WriteLine("Duplicat Control");
            //컨트롤을 복사하고.
            SelectedControlService.CopyControl();

            var band = SelectedControlService.CurrentBand;
            var source = SelectedControlService.CurrentSelectedModel;

            //현재 컨트롤의 가로만큼 우측으로 이동해서
            var location = new Location(source.X + source.Width, source.Y);
           
            //컨트롤을 복제해준다. 
            ControlCreationService.PasteControl(SelectedControlService.CopiedModel, band, location);
        }
        public void PasteControl(bool useLastMousePos)
        {
            if (SelectedControlService.CopiedModel == null)
            {
                Console.WriteLine("복사된 컨트롤이 없습니다.");
                return;
            }
            var band = SelectedControlService.CurrentBand;

            Location loc = null;
            if (useLastMousePos == true)
            {
                loc = new Location(lastMouseX, lastMouseY);
            }

            ControlCreationService.PasteControl(SelectedControlService.CopiedModel, band, loc);
        }
        public void RemoveControl()
        {
            //의미상
            //delete : 복구할수 없음
            //remove : 복구가능

            //1.band 에서 삭제하고
            //2.DesignerOption에서 삭제한다.
            var band = SelectedControlService.CurrentBand;
            band.RemoveSelectedControl();
        }

        public void CutControl()
        {
            //컨트롤을 복사하고,
            SelectedControlService.CopyControl();
            //컨트롤을 삭제한다. 
            RemoveControl();
        }
        public async void OnMenuItemClick(MenuItemEventArgs args)
        {
            var action = args.Text.ToLower();
            if(action == "cut")
            {
                CutControl();
            }
            else if(action  == "remove")
            {
                //컨트롤을 삭제한다. 
                RemoveControl();
            }
            else if (action == "copy")
            {
                SelectedControlService.CopyControl();
            }
            else if(action == "duplicate")
            {
                DuplicateControl();
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
            else if(action == "info")
            {
                Options.FireControlSelectionChangedEvent("ShowRightPanel");
            }
            //todo : 뒤로보내기 앞으로보내기는 컨트롤 z-index로 하나씩 되어있다. 그래서 1씩 변경을 하는데, 첫번째 컨트롤과 맨마지막 컨트롤과의 상하관계를 조절하려면 
            //중간 단계를 전부 수행해줘야 한다. (현재는 수동)
            //이걸 해주려면 컨트롤이 중복되었는지 체크하는 로직이 들어가야 하고, z-index도 상대적으로 변경되어야 할 필요가 있다.
            //성능좀 많이 먹는거 아닐까??
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

                }
                else
                {
                    Console.WriteLine("현재 컨트롤이 제일 위에 있습니다.");
                }
            }
            Options.RefreshBody();

            ContextMenuService.Close();
        }


        
    }
}

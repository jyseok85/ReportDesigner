using Append.Blazor.Clipboard;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using ReportDesigner.Blazor.Common.Data.BaseClass;
using ReportDesigner.Blazor.Common.Data.EtcComponents;
using ReportDesigner.Blazor.Common.UI.ReportControls.Controls;
using ReportDesigner.Blazor.Common.Utils;

namespace ReportDesigner.Blazor.Common.Services
{
    public class ReportContextMenuService
    {
        [Inject]
        public required SelectionService SelectedControlService { get; set; }

        [Inject]
        public required ContextMenuService ContextMenuService { get; set; }

        [Inject]
        public required CreationService ControlCreationService { get; set; }

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
            Logger.Instance.Write($"{args.ClientX} {args.ClientY}");
            Logger.Instance.Write($"{lastMouseX} {lastMouseY}");
            if (SelectedControlService == null)
            {
                Logger.Instance.Write("선택된 컨트롤이 없습니다.");
                return;
            }
            var type = SelectedControlService.CurrentSelectedModel.Type;
            Logger.Instance.Write($"ShowContextMenuWithItems {type}");
            var menuList = new List<ContextMenuItem>();

            int index = 1;

            switch (type)
            {
                case Data.Model.ReportComponentModel.Control.Table:
                case Data.Model.ReportComponentModel.Control.Label:
                    if (SelectedControlService.CurrentSelectedModel.Locked)
                        menuList.Add(CreateMenu("UnLock", "lock_open"));
                    else
                    {
                        menuList.Add(CreateMenu("Lock", "lock"));
                        menuList.Add(CreateMenu("Bring to front", "flip_to_front"));
                        menuList.Add(CreateMenu("Send to back", "flip_to_back"));
                        menuList.Add(CreateMenu("Duplicate", "difference"));
                        menuList.Add(CreateMenu("Remove", "delete")); //휴지통모양
                        menuList.Add(CreateMenu("Cut", "content_cut"));
                        menuList.Add(CreateMenu("Copy", "content_copy"));
                    }
                    break;
                case Data.Model.ReportComponentModel.Control.Report:
                case Data.Model.ReportComponentModel.Control.Layer:
                    break;
                case Data.Model.ReportComponentModel.Control.Band:
                    if (SelectedControlService.CopiedModel is not null)
                        menuList.Add(CreateMenu("Paste", "content_paste"));
                    break;
                case Data.Model.ReportComponentModel.Control.TableCell:
                    if(SelectedControlService.Models.FindAll(x => x.Selected).Count  > 1)
                    {
                        menuList.Add(CreateMenu("Merge Cell", "merge_type"));
                    }
                    else
                    {
                        //todo : 셀분할은 좀 어렵네.. 불가능한게 아니라 셀 분할을 하면 기존 셀들은 분할된 셀만큼 다시 병합된것처럼 rowspan, colspan을 변경해주고 해야한다. 
                        //계산이 엄청 번거로워질듯 하므로 지금 단계에서는 구현하지 않도록 한다. 
                        //찐고급 기능
                        //menuList.Add(CreateMenu("Split Cell", "call_split"));
                    }


                    //todo : 테이블선택 기능 구현 필요
                    menuList.Add(CreateMenu("Select Table", "edit_attributes"));
                    break;

            }

            if (type == Data.Model.ReportComponentModel.Control.Label)
            {
                if(SelectedControlService.CurrentSelectedModel.Locked == false)
                    menuList.Add(CreateMenu("Copy Content", "content_copy"));
            }

            menuList.Add(CreateMenu("Property", "edit_attributes"));//속성창 열기


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
            Logger.Instance.Write("", Microsoft.Extensions.Logging.LogLevel.Debug);
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
                Logger.Instance.Write("복사된 컨트롤이 없습니다.",  Microsoft.Extensions.Logging.LogLevel.Warning);
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
                Logger.Instance.Write($"Clipboard Copied : {text}");
            }
            else if (action == "paste") //부모밴드가 왜 Null?
            {
                PasteControl(true);
            }
            else if(action == "property")
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

                if (targetControls.Count() > 0)
                {

                    //검색된 값중 가장 큰값을 찾는다.
                    var target = targetControls.OrderByDescending(x => x.Model.ZIndex).First();
                    //var target = targetList.Max(x => x.Model.ZIndex);

                    if (target is not null)
                    {
                        //교환하기
                        int old = target.Model.ZIndex;
                        target.Model.ZIndex = zIndex;
                        SelectedControlService.CurrentSelectedModel.ZIndex = old;

                    }
                    else
                    {
                        Logger.Instance.Write("현재 컨트롤이 제일 아래에 있습니다." , Microsoft.Extensions.Logging.LogLevel.Warning);
                    }
                }
                else
                {
                    Logger.Instance.Write("교체할 컨트롤이 없습니다.", Microsoft.Extensions.Logging.LogLevel.Warning);

                }
            }
            else if(action == "bring to front")
            {
                //현재 선택한 컨트롤의 z-index를 가져온다. 
                int zIndex = SelectedControlService.CurrentSelectedModel.ZIndex;

                var controls = SelectedControlService.CurrentBand.controlBases;

                //현재 큰 컨트롤만 찾는다. 
                var targetControls = controls.Where(x => x.Model.ZIndex > zIndex);

                if(targetControls.Count() > 0)
                {

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
                        Logger.Instance.Write("현재 컨트롤이 제일 위에 있습니다." , Microsoft.Extensions.Logging.LogLevel.Warning);
                    }
                }
                else
                {
                    Logger.Instance.Write("교체할 컨트롤이 없습니다." , Microsoft.Extensions.Logging.LogLevel.Warning);
                }
            }
            else if(action.Contains("lock"))
            {
                SelectedControlService.CurrentSelectedModel.Locked = !SelectedControlService.CurrentSelectedModel.Locked;
                Options.FireControlSelectionChangedEvent();
            }
            else if(action == "select table")
            {
                if (SelectedControlService.RazorComponent is not null)
                {
                    //todo : 구리다.. 방법이..
                    ((Table)SelectedControlService.RazorComponent).SelectControl();
                }

            }
            else if(action == "merge cell")
            {
                if (SelectedControlService.RazorComponent is not null)
                {
                    //todo : 구리다.. 방법이..
                    ((Table)SelectedControlService.RazorComponent).MergeCell();
                }
            }
            Options.RefreshBody();

            ContextMenuService.Close();
        }


        
    }
}

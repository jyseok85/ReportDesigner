using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using ReportDesigner.Blazor.Common.Data.BaseClass;
using ReportDesigner.Blazor.Common.UI.ReportControls.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ReportDesigner.Blazor.Common.Services
{
    public class ReportContextMenuService
    {
        [Inject]
        public required SelectedControlService SelectedControl { get; set; }

        [Inject]
        public required ContextMenuService ContextMenuService { get; set; }

        [Inject]
        public required ControlCreationService ControlCreationService { get; set; }

        [Inject]
        public required DesignerOptionService Options { get; set; }

        int lastMouseX = 0;
        int lastMouseY = 0;
        public void ShowContextMenuWithItems(MouseEventArgs args, Point reportLocation)
        {
            lastMouseX = (int)args.ClientX - Options.PaperMargin.Left - (int)reportLocation.X;
            lastMouseY = (int)args.ClientY - Options.PaperMargin.Top - (int)reportLocation.Y;
            //마우스위치는 
            Console.WriteLine($"{args.ClientX} {args.ClientY}");
            Console.WriteLine($"{lastMouseX} {lastMouseY}");
            if (SelectedControl == null)
            {
                Console.WriteLine("선택된 컨트롤이 없습니다.");
                return;
            }
            var type = SelectedControl.CurrentSelectedModel.Type;
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
                    if(SelectedControl.CopiedModel is not null)
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

        async void OnMenuItemClick(MenuItemEventArgs args)
        {
            if(args.Text.ToLower() == "copy")
            {
                SelectedControl.CopyControl();
            }
            if(args.Text.ToLower() == "paste") //부모밴드가 왜 Null?
            {
                if(SelectedControl.CopiedModel == null)
                {
                    Console.WriteLine("복사된 컨트롤이 없습니다.");
                }
                var band = SelectedControl.RazorComponent as BandBase;

                SelectedControl.CopiedModel.X = lastMouseX;
                SelectedControl.CopiedModel.Y = lastMouseY;
                ControlCreationService.PasteControl(SelectedControl.CopiedModel, band);
                SelectedControl.CopiedModel = null;
                Options.RefreshBody();
            }
            //SelectedControl.SetEditMode();
            //var label = SelectedControl.RazorComponent as Control;
            //await label.OnDbClick(null);
            
            Console.WriteLine($"Menu item with Value={args.Value} clicked");
            if (!args.Value.Equals(3) && !args.Value.Equals(4))
            {
                ContextMenuService.Close();
            }
        }
    }
}

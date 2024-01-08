using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportDesigner.Blazor.Common.Services
{
    public class ReportContextMenuService
    {
        [Inject]
        public required SelectedControlService SelectedControl { get; set; }

        [Inject]
        public required ContextMenuService ContextMenuService { get; set; }
        public void ShowContextMenuWithItems(MouseEventArgs args)
        {
            if(SelectedControl == null)
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
                case Data.Model.ReportComponentModel.Control.Label:
                    menuList.Add(CreateMenu("Edit", "home"));
                    menuList.Add(CreateMenu("Lock", "home"));
                    menuList.Add(CreateMenu("Bring to front", "home"));
                    menuList.Add(CreateMenu("Send to back", "home"));
                    menuList.Add(CreateMenu("Copy Content", "info"));
                    menuList.Add(CreateMenu("Duplicate", "home"));
                    break;
                case Data.Model.ReportComponentModel.Control.Report:
                case Data.Model.ReportComponentModel.Control.Layer:
                    break;
                case Data.Model.ReportComponentModel.Control.Band:
                    menuList.Add(CreateMenu("Copy", "search"));
                    break;

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

        void OnMenuItemClick(MenuItemEventArgs args)
        {
            Console.WriteLine($"Menu item with Value={args.Value} clicked");
            if (!args.Value.Equals(3) && !args.Value.Equals(4))
            {
                ContextMenuService.Close();
            }
        }
    }
}

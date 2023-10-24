using Microsoft.AspNetCore.Components.Web;
using ReportDesigner.Blazor.Common.Data.BaseClass;
using ReportDesigner.Blazor.Common.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReportDesigner.Blazor.Common.Services
{
    public class SelectedControlService : ControlBase
    {
        enum Type
        { 
            report,
            layer,
            band,
            control
        }

        public ControlBase Base
        {
            set
            {
                foreach (PropertyInfo propertyInfo in value.GetType().GetProperties())
                {
                    object? obj = propertyInfo.GetValue(value, null);
                    if (null != obj) propertyInfo.SetValue(this, obj, null);
                }
            }

        }

        private List<ReportComponentModel> models = new List<ReportComponentModel>();
        public List<ReportComponentModel> Models => models;
        public void OnPointerDown(PointerEventArgs e, ReportComponentModel model)
        {
            if(e.CtrlKey == false)
            {
                models.ForEach(x => x.Selected = false);
                models.Clear();               
            }
            models.Add(model);
            model.Selected = true;
        }

        public ReportComponentModel LastSelectModel => models[models.Count - 1];

        public void ApplyResize(int x, int y, int width, int height)
        {
            LastSelectModel.X += x;
            LastSelectModel.Y += y;

            if (LastSelectModel.X < 0)
            {
                width += LastSelectModel.X;
                LastSelectModel.X = 0;
            }
            if (LastSelectModel.Y < 0)
            {
                height += LastSelectModel.Y;
                LastSelectModel.Y = 0;
            }
            LastSelectModel.Width = width;
            LastSelectModel.Height = height;
        }
    }
}

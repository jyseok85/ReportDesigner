using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ReportDesigner.Blazor.Common.Data.BaseClass;
using ReportDesigner.Blazor.Common.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ReportDesigner.Blazor.Common.Services
{
    public class SelectedControlService
    {
        enum Type
        { 
            report,
            layer,
            band,
            control
        }
     
        

        private List<ReportComponentModel> models = new List<ReportComponentModel>();
        public List<ReportComponentModel> Models => models;

        //todo : 쓸지 말지 아직 모르겠음.
        private object razorComponent;
        public object RazorComponent => razorComponent;

        public void OnPointerDown(PointerEventArgs e, ReportComponentModel model, object razorComponent = null)
        {
            if(e.CtrlKey == false)
            {
                models.ForEach(x => x.Selected = false);
                models.Clear();               
            }
            models.Add(model);
            model.Selected = true;
            CurrentSelectedModel = model;

            this.razorComponent = razorComponent;

            Console.WriteLine($"SelectedService - OnPointerDown : {CurrentSelectedModel.Name}");
            //오른쪽 속성탭이 열려 있다면 값을 업데이트 해준다.

        }

        public ReportComponentModel LastSelectModel => models[models.Count - 1];

        public void ApplyResize(int x, int y, int width, int height, ReportComponentModel parent)
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


            //오른쪽 밴드 이후 영역으로 나가는지 체크
            if(x >= 0)
            {
                if (width + LastSelectModel.AbsoluteOffsetX > parent.Right + parent.AbsoluteOffsetX)
                {
                    int diff = (width + LastSelectModel.AbsoluteOffsetX) - (parent.Right + parent.AbsoluteOffsetX);
                    width -= diff;
                }
            }

            string msg = $"X:{LastSelectModel.X}, Width:{width}";
            Console.WriteLine(msg);
            LastSelectModel.Width = width;
            LastSelectModel.Height = height;

        }

        public required ReportComponentModel CurrentSelectedModel { get; set; } = new ReportComponentModel();

        public void SetEditMode()
        {
            CurrentSelectedModel.IsEditMode = true;
        }

        public ReportComponentModel CopiedModel = new ReportComponentModel();

        public void CopyControl()
        {
            CopiedModel = CurrentSelectedModel.DeepClone();


        }
        
    }
}

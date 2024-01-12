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
        //private object razorComponent;
        //public object RazorComponent => razorComponent;

        /// <summary>
        /// 변경시점 - 컨트롤을 생성할때, 밴드를 클릭할때
        /// </summary>
        public BandBase? CurrentBand { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="model">선택한 오브젝트</param>
        /// <param name="razorComponent">현재 Razor 오브젝트</param>
        public void OnPointerDown(PointerEventArgs e, ReportComponentModel model, object razorComponent = null)
        {
            SelectControl(e.CtrlKey, model, razorComponent);
        }
        public void SelectControl(bool isMultiSelect, ReportComponentModel model, object parentBand = null)
        {
            Console.WriteLine($"SelectedService - SelectControl : {CurrentSelectedModel.Name} razor {parentBand?.GetType()}");
            if (isMultiSelect == false)
            {
                models.ForEach(x => x.Selected = false);
                models.Clear();
            }
            models.Add(model);
            model.Selected = true;

            CurrentSelectedModel = model;

            if (parentBand is BandBase)
            {
                this.CurrentBand = parentBand as BandBase;
            }

            //this.razorComponent = razorComponent;

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

        public ReportComponentModel? CopiedModel = new ReportComponentModel();

        public void CopyControl()
        {
            CopiedModel = CurrentSelectedModel.DeepClone();
            Console.WriteLine("Control Copied");

        }
        
    }
}

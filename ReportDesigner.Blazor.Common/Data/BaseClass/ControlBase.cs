using Microsoft.AspNetCore.Components;
using ReportDesigner.Blazor.Common.Data.Model;
using ReportDesigner.Blazor.Common.Utils;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;

namespace ReportDesigner.Blazor.Common.Data.BaseClass
{
    public class ControlBase : ComponentBase
    {

        public ReportComponentModel Model { get; set; } = new ReportComponentModel();


        public ControlBase()
        {

        }

        public ControlBase(int x, int y, int width, int height)
        {
            Model.X = x;
            Model.Y = y;
            Model.Width = width;
            Model.Height = height;
        }

        /////상속한 Razor 컴포넌트를 호출하는 기능을 한다. 

        /// <summary>
        /// 상속한 Razor 컴포넌트를 호출하는 기능을 한다.  Copilot 사용
        /// </summary>
        /// <param name="methodName"></param>
        public void CallRazorComponent(string methodName)
        {
            Logger.Instance.Write("", Microsoft.Extensions.Logging.LogLevel.Debug);
            var type = this.GetType();
            var method = type.GetMethod(methodName);
            method?.Invoke(this, null);
        }

        //상속한 Razor 컴포넌트를 호출하는 기능을 한다
        public void CallRazorComponent<T>(string methodName, T value)
        {
            Logger.Instance.Write("", Microsoft.Extensions.Logging.LogLevel.Debug);
            var type = this.GetType();
            var method = type.GetMethod(methodName);
            method?.Invoke(this, new object[] { value });
        }

    }
}

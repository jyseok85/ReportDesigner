using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace ReportDesigner.Blazor.Common.Utils
{
    public class Logger
    {
        private Logger() { }
        //private static 인스턴스 객체
        private static readonly Lazy<Logger> _instance = new Lazy<Logger>(() => new Logger());
        //public static 의 객체반환 함수
        public static Logger Instance { get { return _instance.Value; } }

        public void Write(int msg)
        {
            Write(msg.ToString());
        }


        public void Write(string msg, LogLevel level = LogLevel.Debug)
        {
            if(msg == "")
                Console.WriteLine();


            var stack = GetStacks();
            string className = string.Empty;
            string methodName = string.Empty;
            if (stack != null)
            {
                className = stack[1];
                methodName = stack[2];
            }

            string time = $"{DateTime.Now.ToString("HH:mm:ss fff")}";
            int 공백사이즈 = 20;
            className = string.Format("{0,-" + 공백사이즈 + "}", className);
            if (className.Length > 20)
                className = className.Substring(0, 20);

            methodName = string.Format("{0,-" + 공백사이즈 + "}", methodName);
            if (methodName.Length > 20)
                methodName = methodName.Substring(0, 20);

            string consoleLog = $"[{time}] {className} | {methodName} | {msg}";

            Console.WriteLine(consoleLog);
        }

        private List<string> GetStacks(int stackLevel = 2)
        {
            string? method = null;
            string? nameSpace = null;
            string? className = null;
            var stackFrame = new StackFrame(stackLevel, true);
            MethodBase? methodBase = stackFrame?.GetMethod();

            if (methodBase?.Name == "MoveNext") //코루틴
            {
                method = methodBase?.DeclaringType?.Name;
                nameSpace = methodBase?.DeclaringType?.Namespace;
                className = methodBase?.DeclaringType?.DeclaringType?.Name;
            }
            else
            {
                method = methodBase?.Name;
                className = methodBase?.DeclaringType?.Name;
                nameSpace = methodBase?.DeclaringType?.Namespace;
            }

            if (method is not null && method.Contains("<")) //MEC 코루틴용
            {
                method = method.Replace("<", "");
                int position = method.IndexOf(">");
                method = method.Substring(0, position);
            }

            List<string> rValue = new List<string>();
            if (nameSpace is not null && className is not null && method is not null)
            {
                rValue.Add(nameSpace);
                rValue.Add(className);
                rValue.Add(method);
            }
            return rValue;
        }
    }
}

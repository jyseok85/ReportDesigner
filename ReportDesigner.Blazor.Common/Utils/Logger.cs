using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace ReportDesigner.Blazor.Common.Utils
{
    public class Logger
    {
        


        private Logger() { }
        //private static 인스턴스 객체
        private static readonly Lazy<Logger> _instance = new Lazy<Logger>(() => new Logger());
        //public static 의 객체반환 함수
        public static Logger Instance { get { return _instance.Value; } }

        private int detailLoggingCount = 0;

        private uint loggingCount = 0;

        private LogLevel logLevel = LogLevel.Information;

        private int loggingConfigErrorCount = 0;
        List<string> detailLoggingFunction = new List<string>();


        LogConfig logConfig = new LogConfig();
        LoggingState loggingState = LoggingState.Normal;

        public void Write(int msg)
        {
            Write(msg.ToString());
        }

        public void Write(string msg, LogLevel level = LogLevel.Trace)
        {    
            UpdateLoggingConfig();

            if(IsWriteLog(level) == false)
            {
                return;
            }

            var stack = GetStacks();

            //상세 로그를 사용할지 판단한다.
            if (this.loggingState == LoggingState.Normal && this.logConfig.UseDetailLogging)
            {
                //상세로그 사용일때 원하는 함수가 나타날때까지 기다린다. 
                var isContain = this.detailLoggingFunction.Contains(stack.Method);
                if (isContain)
                {
                    //원하는 함수가 나타나면 로그레벨 체크조건이 상세레벨로 바뀐다. 
                    this.loggingState = LoggingState.Detail;

                    Console.Clear();
                }
            }
           
            WriteConsole(stack.Class, stack.Method, msg, level);

            if (this.loggingState == LoggingState.Detail)
            {
                detailLoggingCount++;

                if (detailLoggingCount > this.logConfig.DetailLogEndValue)
                {
                    //지정한 횟수이상 상세로깅이 되었다면 상세 로깅을 종료한다.
                    this.loggingState = LoggingState.Normal;
                    
                }
            }
        }
        private void WriteConsole(string className, string methodName, string msg, LogLevel level)
        {
            switch(level)
            {
                case LogLevel.Trace:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogLevel.Information:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }

            if (msg == "")
                Console.WriteLine();

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

        /// <summary>
        /// 현재 로그레벨에 따라서 로그를 작성 할지 여부를 계산한다. 
        /// </summary>
        private bool IsWriteLog(LogLevel level)
        {
            if (this.loggingState == LoggingState.Detail)
            {
                if (this.logConfig.LogLevel > level)
                {
                    return false;
                }
            }
            else
            {
                if (this.logLevel > level)
                {
                    return false;
                }
            }
            return true;
        }

        private (string NameSpace, string Class, string Method) GetStacks(int stackLevel = 2)
        {
            string? method ;
            string? nameSpace;
            string? className;
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

            if (nameSpace is not null && className is not null && method is not null)
            {
                return (nameSpace, className,  method);
            }
            return ("", "", "");
        }

        /// <summary>
        /// 일정 주기로 로그 정보를 가져온다.
        /// </summary>
        private void UpdateLoggingConfig()
        {
            if(this.loggingConfigErrorCount > 10)
            {
                return;
            }

            //파일을 읽거나, API를 통해서 로그 정보를 가져온다.

            //일정 시간, 로그라인 주기로 가져오도록 한다.
            string configFilePath = "z:\\test.txt";
            int value = 100;

            try
            {
                if (this.loggingCount % value == 0)
                {
                    var configText = File.ReadAllText(configFilePath);
                    if(configText is not null)
                    {
                        LogConfig config = JsonSerializer.Deserialize<LogConfig?>(configText);
                        if (config is not null)
                        {
                            this.logLevel = config.LogLevel;
                            this.logConfig = config;
                            this.detailLoggingFunction = config.DetailLogFunction.Split(',').ToList();
                        }
                    }

                    var beforeColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine(configText);
                    Console.ForegroundColor = beforeColor;
                }
                this.loggingCount++;

            }
            catch (Exception ex)
            {
                this.loggingConfigErrorCount++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
            }
        }

        enum LoggingState
        {
            None,
            Normal,
            Detail
        }
    }

    public class LogConfig
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LogLevel LogLevel { get; set; }
        public bool UseDetailLogging { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LogLevel DetailLogLevel { get; set; }
        public string DetailLogFunction { get; set; } = string.Empty;
        public LoggingCondition DetailLogCondition { get; set; }

        public int DetailLogEndValue { get; set; }

    }

    public enum LoggingCondition
    {
        Time,
        Count
    }
}

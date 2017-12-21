using log4net;
using System;
using System.Globalization;

namespace MZ.Jobs.Core
{
    /// <summary>
    /// 日志
    /// </summary>
    public   class JobLogger
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(JobLogger));
        public static void Info(string content)
        {
            Console.WriteLine($"{DateTime.Now}{content}V7");
            content = content.Replace("{", "[").Replace("}", "]");
             Logger.InfoFormat(content);

        }
        public static void Info(string format,params object[] args)
        {
            Console.WriteLine(DateTime.Now.ToString(CultureInfo.CurrentCulture)+string.Format(format, args));
          Logger.InfoFormat(format,args);

        }
        public static void ApiExecInfo(string format, params object[] args)
        {
            if (CustomerConfig.ShowApiLog)
            {
                Info(format, args);
            }
        }
        

    }
}

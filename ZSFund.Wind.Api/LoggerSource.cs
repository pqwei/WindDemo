using System;
using System.Collections.Generic;
using System.Text;
using ZSFund.LogService;

namespace ZSFund.Wind.Api
{
    public class LoggerSource
    {
        public static readonly string DefaultAppSource = "ZSFund.Wind.Api";
    }
    /// <summary>
    /// 写日志
    /// </summary>
    public static class Logger
    {
        private static ILogService logger = new ZSFund.LogService.Api.Logger();
        public static bool Write(string title, string message, string logType, string logType2, LogLevel level, int priority, string loginName = null, string clientIP = null)
        {
            return logger.Write(LoggerSource.DefaultAppSource, title, message, logType, logType2, level, priority, loginName, clientIP).GetAwaiter().GetResult();
        }
    }
}

using Newtonsoft.Json;
using NLog;
using Sidekick.Model;
using System;

namespace Sidekick.Api.Helpers
{
    public class LoggerManager : ILoggerManager
    {
        private ILogger logger = LogManager.GetCurrentClassLogger();

        public LoggerManager()
        {
        }

        public void LogDebugObject(object obj)
        {
            if (obj != null) logger.Info("\n----- Object Inspect: -----\n" + JsonConvert.SerializeObject(obj, Formatting.Indented).ToString() + "\n---------------------------");
        }

        public void LogDebug(string message)
        {
            logger.Debug(message);
        }

        public void LogError(string message)
        {
            logger.Error(message);
        }

        public void LogInfo(string message)
        {
            logger.Info(message);
        }

        public void LogInfo(ETransaction transaction, string methodName, EOperation operation)
        {
            logger.Info($"-- {transaction}::{methodName}::{operation} --");
        }

        public void LogWarn(string message)
        {
            logger.Warn(message);
        }

        public void LogException(Exception ex)
        {
            if (ex.InnerException != null)
            {
                if (ex.InnerException.Message != null)
                {
                    LogError(ex.InnerException.Message);
                }
            }
            LogError(ex.StackTrace);
        }
    }
}

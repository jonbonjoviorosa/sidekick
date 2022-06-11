using Sidekick.Model;
using System;

namespace Sidekick.Api.Helpers
{
    public interface ILoggerManager
    {
        void LogDebugObject(object obj);
        void LogInfo(string message);
        void LogInfo(ETransaction transaction, string methodName, EOperation operation);
        void LogWarn(string message);
        void LogDebug(string message);
        void LogError(string message);
        void LogException(Exception ex);
    }
}

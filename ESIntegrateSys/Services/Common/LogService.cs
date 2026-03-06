using System;
using System.Threading.Tasks;
using ESIntegrateSys.Utilities;

namespace ESIntegrateSys.Services.Common
{
    /// <summary>
    /// 日誌服務實現
    /// </summary>
    public class LogService : ILogService
    {
        /// <summary>
        /// 記錄信息
        /// </summary>
        /// <param name="message">信息內容</param>
        /// <param name="logType">日誌類型</param>
        public void LogInfo(string message, string logType = "Info")
        {
            LogHelper.LogInfo(message, logType);
        }

        /// <summary>
        /// 記錄錯誤
        /// </summary>
        /// <param name="ex">異常</param>
        /// <param name="additionalInfo">附加信息</param>
        public void LogError(Exception ex, string additionalInfo = "")
        {
            LogHelper.LogError(ex, additionalInfo);
        }

        /// <summary>
        /// 記錄警告
        /// </summary>
        /// <param name="message">警告信息</param>
        public void LogWarning(string message)
        {
            LogHelper.LogWarning(message);
        }

        /// <summary>
        /// 非同步記錄信息
        /// </summary>
        /// <param name="message">信息內容</param>
        /// <param name="logType">日誌類型</param>
        public async Task LogInfoAsync(string message, string logType = "Info")
        {
            await LogHelper.LogInfoAsync(message, logType).ConfigureAwait(false);
        }

        /// <summary>
        /// 非同步記錄錯誤
        /// </summary>
        /// <param name="ex">異常</param>
        /// <param name="additionalInfo">附加信息</param>
        public async Task LogErrorAsync(Exception ex, string additionalInfo = "")
        {
            await LogHelper.LogErrorAsync(ex, additionalInfo).ConfigureAwait(false);
        }

        /// <summary>
        /// 非同步記錄警告
        /// </summary>
        /// <param name="message">警告信息</param>
        public async Task LogWarningAsync(string message)
        {
            await LogHelper.LogWarningAsync(message).ConfigureAwait(false);
        }
    }
}

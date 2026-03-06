using System;
using System.Threading.Tasks;

namespace ESIntegrateSys.Services.Common
{
    /// <summary>
    /// 日誌服務接口
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// 記錄信息
        /// </summary>
        /// <param name="message">信息內容</param>
        /// <param name="logType">日誌類型</param>
        void LogInfo(string message, string logType = "Info");

        /// <summary>
        /// 記錄錯誤
        /// </summary>
        /// <param name="ex">異常</param>
        /// <param name="additionalInfo">附加信息</param>
        void LogError(Exception ex, string additionalInfo = "");

        /// <summary>
        /// 記錄警告
        /// </summary>
        /// <param name="message">警告信息</param>
        void LogWarning(string message);

        /// <summary>
        /// 非同步記錄信息
        /// </summary>
        /// <param name="message">信息內容</param>
        /// <param name="logType">日誌類型</param>
        Task LogInfoAsync(string message, string logType = "Info");

        /// <summary>
        /// 非同步記錄錯誤
        /// </summary>
        /// <param name="ex">異常</param>
        /// <param name="additionalInfo">附加信息</param>
        Task LogErrorAsync(Exception ex, string additionalInfo = "");

        /// <summary>
        /// 非同步記錄警告
        /// </summary>
        /// <param name="message">警告信息</param>
        Task LogWarningAsync(string message);
    }
}

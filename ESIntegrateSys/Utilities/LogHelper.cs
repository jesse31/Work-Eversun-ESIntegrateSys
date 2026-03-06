using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ESIntegrateSys.Utilities
{
    /// <summary>
    /// 日誌記錄輔助類
    /// </summary>
    public static class LogHelper
    {
        private static readonly object _lockObj = new object();
        private static readonly string _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        /// <summary>
        /// 記錄信息
        /// </summary>
        /// <param name="message">信息內容</param>
        /// <param name="logType">日誌類型</param>
        public static void LogInfo(string message, string logType = "Info")
        {
            WriteLog(message, logType);
        }

        /// <summary>
        /// 記錄錯誤
        /// </summary>
        /// <param name="ex">異常</param>
        /// <param name="additionalInfo">附加信息</param>
        public static void LogError(Exception ex, string additionalInfo = "")
        {
            string message = $"{additionalInfo}\r\n錯誤信息: {ex.Message}\r\n堆疊跟踪: {ex.StackTrace}";
            WriteLog(message, "Error");
        }

        /// <summary>
        /// 記錄警告
        /// </summary>
        /// <param name="message">警告信息</param>
        public static void LogWarning(string message)
        {
            WriteLog(message, "Warning");
        }

        /// <summary>
        /// 非同步記錄信息
        /// </summary>
        /// <param name="message">信息內容</param>
        /// <param name="logType">日誌類型</param>
        public static async Task LogInfoAsync(string message, string logType = "Info")
        {
            await WriteLogAsync(message, logType).ConfigureAwait(false);
        }

        /// <summary>
        /// 非同步記錄錯誤
        /// </summary>
        /// <param name="ex">異常</param>
        /// <param name="additionalInfo">附加信息</param>
        public static async Task LogErrorAsync(Exception ex, string additionalInfo = "")
        {
            string message = $"{additionalInfo}\r\n錯誤信息: {ex.Message}\r\n堆疊跟踪: {ex.StackTrace}";
            await WriteLogAsync(message, "Error").ConfigureAwait(false);
        }

        /// <summary>
        /// 非同步記錄警告
        /// </summary>
        /// <param name="message">警告信息</param>
        public static async Task LogWarningAsync(string message)
        {
            await WriteLogAsync(message, "Warning").ConfigureAwait(false);
        }

        /// <summary>
        /// 寫入日誌
        /// </summary>
        /// <param name="message">日誌內容</param>
        /// <param name="logType">日誌類型</param>
        private static void WriteLog(string message, string logType)
        {
            try
            {
                if (!Directory.Exists(_logPath))
                {
                    Directory.CreateDirectory(_logPath);
                }

                string fileName = $"{DateTime.Now:yyyy-MM-dd}_{logType}.log";
                string filePath = Path.Combine(_logPath, fileName);
                string logContent = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\r\n";

                lock (_lockObj)
                {
                    File.AppendAllText(filePath, logContent, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"寫入日誌時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 非同步寫入日誌
        /// </summary>
        /// <param name="message">日誌內容</param>
        /// <param name="logType">日誌類型</param>
        private static async Task WriteLogAsync(string message, string logType)
        {
            try
            {
                if (!Directory.Exists(_logPath))
                {
                    Directory.CreateDirectory(_logPath);
                }

                string fileName = $"{DateTime.Now:yyyy-MM-dd}_{logType}.log";
                string filePath = Path.Combine(_logPath, fileName);
                string logContent = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\r\n";

                // 使用非同步寫入
                using (StreamWriter writer = new StreamWriter(filePath, true, Encoding.UTF8))
                {
                    await writer.WriteAsync(logContent).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"非同步寫入日誌時發生錯誤: {ex.Message}");
            }
        }
    }
}

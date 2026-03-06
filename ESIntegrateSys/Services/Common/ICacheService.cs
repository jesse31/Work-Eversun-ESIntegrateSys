using System;
using System.Threading.Tasks;

namespace ESIntegrateSys.Services.Common
{
    /// <summary>
    /// 快取服務接口
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// 獲取快取項目，如果不存在則使用提供的函數創建
        /// </summary>
        /// <typeparam name="T">快取項目類型</typeparam>
        /// <param name="key">快取鍵</param>
        /// <param name="getItemFunc">獲取項目的函數</param>
        /// <param name="slidingExpiration">滑動過期時間（分鐘）</param>
        /// <returns>快取項目</returns>
        T GetOrCreate<T>(string key, Func<T> getItemFunc, int slidingExpiration = 30);
        
        /// <summary>
        /// 非同步獲取快取項目，如果不存在則使用提供的函數創建
        /// </summary>
        /// <typeparam name="T">快取項目類型</typeparam>
        /// <param name="key">快取鍵</param>
        /// <param name="getItemFunc">非同步獲取項目的函數</param>
        /// <param name="slidingExpiration">滑動過期時間（分鐘）</param>
        /// <returns>快取項目</returns>
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> getItemFunc, int slidingExpiration = 30);
        
        /// <summary>
        /// 移除快取項目
        /// </summary>
        /// <param name="key">快取鍵</param>
        void Remove(string key);
        
        /// <summary>
        /// 清除所有快取
        /// </summary>
        void Clear();
        
        /// <summary>
        /// 獲取快取項目，如果不存在則返回默認值
        /// </summary>
        /// <typeparam name="T">快取項目類型</typeparam>
        /// <param name="key">快取鍵</param>
        /// <returns>快取項目或默認值</returns>
        T Get<T>(string key);
        
        /// <summary>
        /// 設置快取項目
        /// </summary>
        /// <typeparam name="T">快取項目類型</typeparam>
        /// <param name="key">快取鍵</param>
        /// <param name="value">快取值</param>
        /// <param name="slidingExpiration">滑動過期時間（分鐘）</param>
        void Set<T>(string key, T value, int slidingExpiration = 30);
        
        /// <summary>
        /// 設置快取項目，使用絕對過期時間
        /// </summary>
        /// <typeparam name="T">快取項目類型</typeparam>
        /// <param name="key">快取鍵</param>
        /// <param name="value">快取值</param>
        /// <param name="absoluteExpiration">絕對過期時間</param>
        void SetWithAbsoluteExpiration<T>(string key, T value, DateTime absoluteExpiration);
    }
}

using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using ESIntegrateSys.Utilities;

namespace ESIntegrateSys.Services.Common
{
    /// <summary>
    /// 快取服務實現
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly ObjectCache _cache = MemoryCache.Default;
        
        /// <summary>
        /// 獲取快取項目，如果不存在則使用提供的函數創建
        /// </summary>
        /// <typeparam name="T">快取項目類型</typeparam>
        /// <param name="key">快取鍵</param>
        /// <param name="getItemFunc">獲取項目的函數</param>
        /// <param name="slidingExpiration">滑動過期時間（分鐘）</param>
        /// <returns>快取項目</returns>
        public T GetOrCreate<T>(string key, Func<T> getItemFunc, int slidingExpiration = 30)
        {
            return CacheHelper.GetOrCreate(key, getItemFunc, slidingExpiration);
        }
        
        /// <summary>
        /// 非同步獲取快取項目，如果不存在則使用提供的函數創建
        /// </summary>
        /// <typeparam name="T">快取項目類型</typeparam>
        /// <param name="key">快取鍵</param>
        /// <param name="getItemFunc">非同步獲取項目的函數</param>
        /// <param name="slidingExpiration">滑動過期時間（分鐘）</param>
        /// <returns>快取項目</returns>
        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> getItemFunc, int slidingExpiration = 30)
        {
            return await CacheHelper.GetOrCreateAsync(key, getItemFunc, slidingExpiration).ConfigureAwait(false);
        }
        
        /// <summary>
        /// 移除快取項目
        /// </summary>
        /// <param name="key">快取鍵</param>
        public void Remove(string key)
        {
            CacheHelper.Remove(key);
        }
        
        /// <summary>
        /// 清除所有快取
        /// </summary>
        public void Clear()
        {
            CacheHelper.Clear();
        }
        
        /// <summary>
        /// 獲取快取項目，如果不存在則返回默認值
        /// </summary>
        /// <typeparam name="T">快取項目類型</typeparam>
        /// <param name="key">快取鍵</param>
        /// <returns>快取項目或默認值</returns>
        public T Get<T>(string key)
        {
            if (_cache.Contains(key))
            {
                return (T)_cache.Get(key);
            }
            
            return default(T);
        }
        
        /// <summary>
        /// 設置快取項目
        /// </summary>
        /// <typeparam name="T">快取項目類型</typeparam>
        /// <param name="key">快取鍵</param>
        /// <param name="value">快取值</param>
        /// <param name="slidingExpiration">滑動過期時間（分鐘）</param>
        public void Set<T>(string key, T value, int slidingExpiration = 30)
        {
            CacheItemPolicy policy = new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromMinutes(slidingExpiration)
            };
            
            _cache.Set(key, value, policy);
        }
        
        /// <summary>
        /// 設置快取項目，使用絕對過期時間
        /// </summary>
        /// <typeparam name="T">快取項目類型</typeparam>
        /// <param name="key">快取鍵</param>
        /// <param name="value">快取值</param>
        /// <param name="absoluteExpiration">絕對過期時間</param>
        public void SetWithAbsoluteExpiration<T>(string key, T value, DateTime absoluteExpiration)
        {
            CacheItemPolicy policy = new CacheItemPolicy
            {
                AbsoluteExpiration = absoluteExpiration
            };
            
            _cache.Set(key, value, policy);
        }
    }
}

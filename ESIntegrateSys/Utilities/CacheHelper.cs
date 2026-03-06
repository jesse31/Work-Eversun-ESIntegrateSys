using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace ESIntegrateSys.Utilities
{
    /// <summary>
    /// 快取輔助類
    /// </summary>
    public static class CacheHelper
    {
        private static readonly ObjectCache _cache = MemoryCache.Default;

        /// <summary>
        /// 獲取快取項目，如果不存在則使用提供的函數創建
        /// </summary>
        /// <typeparam name="T">快取項目類型</typeparam>
        /// <param name="key">快取鍵</param>
        /// <param name="getItemFunc">獲取項目的函數</param>
        /// <param name="slidingExpiration">滑動過期時間（分鐘）</param>
        /// <returns>快取項目</returns>
        public static T GetOrCreate<T>(string key, Func<T> getItemFunc, int slidingExpiration = 30)
        {
            // 檢查快取中是否存在
            if (_cache.Contains(key))
            {
                return (T)_cache.Get(key);
            }

            // 創建項目
            T item = getItemFunc();

            // 添加到快取
            CacheItemPolicy policy = new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromMinutes(slidingExpiration)
            };

            _cache.Set(key, item, policy);

            return item;
        }

        /// <summary>
        /// 非同步獲取快取項目，如果不存在則使用提供的函數創建
        /// </summary>
        /// <typeparam name="T">快取項目類型</typeparam>
        /// <param name="key">快取鍵</param>
        /// <param name="getItemFunc">非同步獲取項目的函數</param>
        /// <param name="slidingExpiration">滑動過期時間（分鐘）</param>
        /// <returns>快取項目</returns>
        public static async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> getItemFunc, int slidingExpiration = 30)
        {
            // 檢查快取中是否存在
            if (_cache.Contains(key))
            {
                return (T)_cache.Get(key);
            }

            // 非同步創建項目
            T item = await getItemFunc().ConfigureAwait(false);

            // 添加到快取
            CacheItemPolicy policy = new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromMinutes(slidingExpiration)
            };

            _cache.Set(key, item, policy);

            return item;
        }

        /// <summary>
        /// 移除快取項目
        /// </summary>
        /// <param name="key">快取鍵</param>
        public static void Remove(string key)
        {
            if (_cache.Contains(key))
            {
                _cache.Remove(key);
            }
        }

        /// <summary>
        /// 清除所有快取
        /// </summary>
        public static void Clear()
        {
            List<string> cacheKeys = new List<string>();

            // 獲取所有快取鍵
            foreach (KeyValuePair<string, object> item in _cache)
            {
                cacheKeys.Add(item.Key);
            }

            // 移除所有快取項目
            foreach (string key in cacheKeys)
            {
                _cache.Remove(key);
            }
        }
    }
}

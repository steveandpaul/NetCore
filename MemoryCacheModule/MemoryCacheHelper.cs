using System;
using System.Runtime.Caching;

namespace MemoryCacheModule.InMemoryCache
{
    /// <summary>
    /// 基于MemoryCache的缓存辅助类 这里其实可以做成静态类和里面的方法做成静态方法，不需要依赖注入
    /// 但是这里为了框架的一致性使用了依赖注入，把这个缓存类做成了非静态
    /// </summary>
    public class MemoryCacheHelper
    {
        private static readonly Object _locker = new object();

        public  T GetCacheItem<T>(String key, Func<T> cachePopulate, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null)
        {
          
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentException("Invalid cache key");
            //if (cachePopulate == null) throw new ArgumentNullException("cachePopulate");
            //if (slidingExpiration == null && absoluteExpiration == null) throw new ArgumentException("Either a sliding expiration or absolute must be provided");

            if (MemoryCache.Default[key] == null)
            {
                lock (_locker)
                {
                    if (MemoryCache.Default[key] == null)
                    {
                        var item = new CacheItem(key, cachePopulate());
                        var policy = CreatePolicy(slidingExpiration, absoluteExpiration);

                        MemoryCache.Default.Add(item, policy);
                    }
                }
            }

            return (T)MemoryCache.Default[key];
        }

        private  CacheItemPolicy CreatePolicy(TimeSpan? slidingExpiration, DateTime? absoluteExpiration)
        {
            var policy = new CacheItemPolicy();

            if (absoluteExpiration.HasValue)
            {
                policy.AbsoluteExpiration = absoluteExpiration.Value;
            }
            else if (slidingExpiration.HasValue)
            {
                policy.SlidingExpiration = slidingExpiration.Value;
            }

            policy.Priority = CacheItemPriority.Default;

            return policy;
        }

        public  void CacheRemove(string strKey)
        {
            if (string.IsNullOrWhiteSpace(strKey)) throw new ArgumentException("无效键");
            if (MemoryCache.Default[strKey] != null)
            {
                lock (_locker)
                {
                    MemoryCache.Default.Remove(strKey);
                }
            }
        }
    }

}


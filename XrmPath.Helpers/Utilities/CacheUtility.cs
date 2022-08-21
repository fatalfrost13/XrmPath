using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace XrmPath.Helpers.Utilities
{
    public static class CacheUtility
    {
        private static ObjectCache Cache { get; } = MemoryCache.Default;
        //private static readonly object GlobalCacheUtilityLock = new object();

        /// <summary>
        /// Gets Cache object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Cache Key</param>
        /// <param name="addToCacheFunction">Function to use if cache is null</param>
        /// <param name="lockObject">Lock object. If null it will use the global cache object in CacheUtility</param>
        /// <param name="cacheInHours">Hours the cache will stay before it expires</param>
        /// <param name="logAction">True will log if cache gets created. Default is true</param>
        /// <param name="readFromFile">Will read and write cache into file to presist it in case of Application Restart or IIS App pool recycle</param>
        /// <returns></returns>
        public static T GetWithLock<T>(string key, Func<T> addToCacheFunction, object lockObject = null, int cacheInHours = 24, bool logAction = true, bool readFromFile = true)
        {
            var cacheItem = Cache.Contains(key) && Cache.Get(key) != null ? Cache[key] : null;

            if (cacheItem != null)
            {
                return (T)cacheItem;
            }

            if (lockObject != null)
            {
                lock (lockObject)
                {
                    //need to check once more due to multiple requests into the lock
                    return Get(key, addToCacheFunction, cacheInHours, logAction, readFromFile);
                }
            }
            else
            {
                return Get(key, addToCacheFunction, cacheInHours, logAction, readFromFile);
            }
        }

        public static T Get<T>(string key, Func<T> addToCacheFunction,  int cacheInHours = 24, bool logAction = true, bool readFromFile = true)
        {
            var cacheItem = Cache.Contains(key) && Cache.Get(key) != null ? Cache[key] : null;
            if (cacheItem == null)
            {

                //cacheItem = addToCacheFunction();
                if (readFromFile)
                {
                    //read from file
                    cacheItem = CacheFileUtility.Get(key, addToCacheFunction, cacheInHours, logAction);
                }
                else
                {
                    if (logAction)
                    {
                        Serilog.Log.Information($"XrmPath.Helpers Cache ({key}): Starting Creating Cache");
                        //LogHelper.Info<string>($"XrmPath.Helpers Cache ({key}): Starting Creating Cache");
                    }

                    cacheItem = addToCacheFunction();

                    if (logAction)
                    {
                        Serilog.Log.Information($"XrmPath.Helpers Cache ({key}): Completed Building Cache. Expires in {cacheInHours} Hours");
                        //LogHelper.Info<string>($"XrmPath.Helpers Cache ({key}): Completed Building Cache. Expires in {cacheInHours} Hours");
                    }
                }

                if (cacheItem != null)
                {
                    var policyLastUpdate = new CacheItemPolicy()
                    {
                        AbsoluteExpiration = DateTimeOffset.Now.AddHours(cacheInHours)
                    };
                    CacheByKey(key, cacheItem, policyLastUpdate);
                }

            }
            return (T)cacheItem;
        }

        /// <summary>
        /// Creates Cache Object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Cache Key</param>
        /// <param name="value">Cache Value</param>
        /// <param name="lockObject">Lock object. If null it will use the global cache object in CacheUtility</param>
        /// <param name="cacheInHours">Hours the cache will stay before it expires</param>
        /// <param name="logAction">True will log if cache gets created. Default is False</param>
        /// <param name="writeToFile"></param>
        /// <returns></returns>
        public static T SetWithLock<T>(string key, object value, object lockObject = null, int cacheInHours = 24, bool logAction = false, bool writeToFile = false)
        {
            var cacheItem = Cache.Contains(key) && Cache.Get(key) != null ? Cache[key] : null;

            if (lockObject != null)
            {
                if (cacheItem != null)
                {
                    RemoveWithLock(key, lockObject, logAction);
                }
                lock (lockObject)
                {
                    //need to check once more due to multiple requests into the lock
                    return Set<T>(key, value, cacheInHours, logAction, writeToFile);
                }
            }
            else
            {
                //no lock specified
                if (cacheItem != null)
                {
                    Remove(key);
                }
                return Set<T>(key, value, cacheInHours, logAction, writeToFile);
            }
        }

        public static T Set<T>(string key, object value, int cacheInHours = 24, bool logAction = false, bool writeToFile = false)
        {
            var cacheItem = Cache.Contains(key) && Cache.Get(key) != null ? Cache[key] : null;
            if (cacheItem == null)
            {
                if (logAction)
                {
                    Serilog.Log.Information($"XrmPath Cache ({key}): Starting Creating Cache");
                    //LogHelper.Info<string>($"XrmPath Cache ({key}): Starting Creating Cache");
                }

                cacheItem = value;
                var policyLastUpdate = new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.Now.AddHours(cacheInHours) };

                if (writeToFile)
                {
                    CacheFileUtility.CacheByKey<T>(key, value, true);
                }

                CacheByKey(key, cacheItem, policyLastUpdate);

                if (logAction)
                {
                    Serilog.Log.Information($"XrmPath.Helpers Cache ({key}): Completed Building Cache. Expires in {cacheInHours} Hours");
                    //LogHelper.Info<string>($"XrmPath.Helpers Cache ({key}): Completed Building Cache. Expires in {cacheInHours} Hours");
                }
            }
            return (T)cacheItem;
        }

        private static void CacheByKey(string key, object result, CacheItemPolicy policyLastUpdate)
        {
            Cache.Add(new CacheItem(key, result), policyLastUpdate);
        }

        /// <summary>
        /// Removes Cache Object
        /// </summary>
        /// <param name="key">Cache Key</param>
        /// <param name="lockObject">Lock object. If null it will use the global cache object in CacheUtility</param>
        /// <param name="logAction">True will log if cache gets removed. Default is False</param>
        /// <param name="removeFile"></param>
        public static void RemoveWithLock(string key, object lockObject = null, bool logAction = true, bool removeFile = false)
        {
            if (lockObject != null)
            {
                lock (lockObject)
                {
                    Remove(key, removeFile);
                }
            }
            else
            {
                Remove(key, removeFile);
            }

            if (logAction)
            {
                Serilog.Log.Information($"XrmPath.Helpers Cache ({key}): Removed from Cache");
                //LogHelper.Info<string>($"XrmPath.Helpers Cache ({key}): Removed from Cache");
            }
        }

        public static void Remove(string key, bool removeFile = false)
        {
            if (Cache.Contains(key))
            {
                Cache.Remove(key);
            }
            if (removeFile)
            {
                CacheFileUtility.Remove(key);
            }
        }

        public static void RemoveAllKeyStartsWith(string keyStartsWith, bool removeFile = false)
        {
            var cacheToRemove = Cache.Where(i => i.Key.StartsWith(keyStartsWith)).Select(i => i.Key).ToList();
            foreach(var cacheKey in cacheToRemove)
            {
                Remove(cacheKey, removeFile);
            }
        }

        public static List<string> GetAllKeyStartsWith(string keyStartsWith) {
            var cacheList = Cache.Where(i => i.Key.StartsWith(keyStartsWith)).Select(i => i.Key).ToList();
            return cacheList;
        }

        public static bool CacheExists(string key)
        {
            var cacheExists = (Cache.Contains(key) && Cache.Get(key) != null);
            return cacheExists;
        }
    }
}
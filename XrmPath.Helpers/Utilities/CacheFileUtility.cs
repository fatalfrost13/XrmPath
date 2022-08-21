using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using XrmPath.Helpers.Model;
using XrmPath.Helpers.Serializers;
using System.Collections.Generic;

namespace XrmPath.Helpers.Utilities
{
    public static class CacheFileUtility
    {
        private static ObjectCache Cache { get; } = MemoryCache.Default;
        //private static readonly object GlobalCacheUtilityLock = new object();

        public static T Get<T>(string key, Func<T> addToCacheFunction, int cacheInHours = 24, bool logAction = false, bool updateTracker = true)
        {

            object cacheItem;
            var fileInfo = GetCacheFileInfo(key);
            var utcNow = DateTime.UtcNow;

            if (!fileInfo.Exists || fileInfo.CreationTime.ToUniversalTime().AddHours(cacheInHours) < utcNow)
            {
                if (fileInfo.Exists)
                {
                    //file has expired, so delete to create a new one.
                    fileInfo.Delete();
                }
                cacheItem = BuildCache(key, addToCacheFunction, cacheInHours, logAction, true);
            }
            else
            {
                // Method 1: read file into a string and deserialize JSON to a type
                //cacheItem = JsonConvert.DeserializeObject<T>(File.ReadAllText(fileInfo.FullName));
                //cacheItem = JilSerializer.DeserializeObject<T>(File.ReadAllText(fileInfo.FullName));
                //cacheItem = ProtobufSerializer.Deserialize<T>(File.ReadAllBytes(fileInfo.FullName));

                // Method 2: deserialize JSON directly from a file
                cacheItem = FileSerializer.DeserializeFromFile<T>(fileInfo);

                if (cacheItem == null)
                {
                    cacheItem = BuildCache(key, addToCacheFunction, cacheInHours, logAction);
                }

                if (updateTracker)
                {
                    //fileInfo = new FileInfo(file);
                    var file = GetCacheFilePath(key);
                    var updatedFileInfo = new FileInfo(file);
                    SetCacheStatus(key, cacheInHours, updatedFileInfo.LastWriteTimeUtc);
                }

            }

            return (T)cacheItem;
        }

        private static T BuildCache<T>(string key, Func<T> addToCacheFunction, int cacheInHours = 24, bool logAction = false, bool fileRemoved = false)
        {
            if (logAction)
            {
                Serilog.Log.Information($"XrmPath.Helpers Cache ({key}): Starting Creating Cache");
                //LogHelper.Info<string>($"XrmPath.Helpers Cache ({key}): Starting Creating Cache");
            }

            object cacheItem = addToCacheFunction();
            if (cacheItem != null)
            {
                CacheByKey<T>(key, cacheItem, fileRemoved);
            }

            if (logAction)
            {
                Serilog.Log.Information($"XrmPath.Helpers Cache ({key}): Completed Building Cache. Expires in {cacheInHours} Hours");
                //LogHelper.Info<string>($"XrmPath.Helpers Cache ({key}): Completed Building Cache. Expires in {cacheInHours} Hours");
            }

            return (T)cacheItem;
        }

        public static T Set<T>(string key, object value)
        {
            object cacheItem = null;

            var file = GetCacheFileInfo(key);
            if (file.Exists)
            {
                file.Delete();
            }

            //need to check once more due to multiple requests into the lock
            if (value != null)
            {
                cacheItem = value;
                CacheByKey<T>(key, cacheItem, true);
            }
            return (T)cacheItem; 
            
        }

        public static void CacheByKey<T>(string key, object result, bool fileRemoved = false)
        {
            var file = GetCacheFileInfo(key);

            //Method 1: serialize JSON to a string the write to a file
            //File.WriteAllText(file, JsonConvert.SerializeObject((T)result));
            //File.WriteAllText(file, JilSerializer.SerializeObject((T)result));
            //File.WriteAllBytes(file, ProtobufSerializer.Serialize(result));

            //Method 2: serialize JSON direct to a file
            var fileCreated = FileSerializer.SerializeToFile<T>(file, result);

            if (fileRemoved && fileCreated)
            {
                var utcNow = DateTime.UtcNow;
                //if file had just been deleted, set the file created to utcnow
                File.SetCreationTimeUtc(file.FullName, utcNow);
                File.SetLastWriteTimeUtc(file.FullName, utcNow);
            }

            //update cache tracker
            SetCacheStatus(key);
        }

        public static void Remove(string key)
        {
            //var file = GetCacheFilePath(key);
            var fileInfo = GetCacheFileInfo(key);
            if (fileInfo.Exists)
            {
                //content is expired. delete file
                fileInfo.Delete(); 
            }
        }

        public static bool CacheExists(string key, int cacheInHours)
        {
            var cacheExists = true;
            var fileInfo = GetCacheFileInfo(key);
            var utcNow = DateTime.UtcNow;

            var compareDate = fileInfo.CreationTimeUtc;

            if (!fileInfo.Exists || compareDate.AddHours(cacheInHours) < utcNow)
            {
                cacheExists = false;
            }

            return cacheExists;
        }

        private static string GetCacheFilePath(string key)
        {
            var cacheFilePath = $"{ConfigurationManager.AppSettings["CachedDataPath"]}{key}.json";
            var file = MapPath(cacheFilePath);

            return file;
        }

        private static string MapPath(string cacheFilePath)
        {
            var hostingRoot = System.Web.Hosting.HostingEnvironment.IsHosted ? System.Web.Hosting.HostingEnvironment.MapPath("~/") : AppDomain.CurrentDomain.BaseDirectory;

            if (string.IsNullOrEmpty(hostingRoot))
            {
                hostingRoot = string.Empty;
            }

            return Path.Combine(hostingRoot, cacheFilePath.Substring(1).Replace('/', '\\'));
        }

        public static FileInfo GetCacheFileInfo(string key)
        {
            var file = GetCacheFilePath(key);
            var fileInfo = new FileInfo(file);
            return fileInfo;
        }

        public static List<FileInfo> GetCacheFileInfoList(string keyStartsWith)
        {
            var fileList = new List<FileInfo>();
            var cacheFolder = GetCacheFolder();
            foreach(var file in cacheFolder.GetFiles())
            {
                if (file.Name.StartsWith(keyStartsWith))
                {
                    fileList.Add(file);
                }
            }
            return fileList;
        }

        public static DirectoryInfo GetCacheFolder()
        {
            var cacheFolderPath = $"{ConfigurationManager.AppSettings["CachedDataPath"]}";
            var folder = MapPath(cacheFolderPath);
            var folderInfo = new DirectoryInfo(folder);
            return folderInfo;
        }

        public static CacheTracker GetCacheStatus(string key, int cacheInHours = 24)
        {
            var fileName = $"{key}.json";
            var cacheItem = Cache.Contains(fileName) && Cache.Get(fileName) != null ? (CacheTracker)Cache[fileName] : null;

            if (cacheItem != null)
            {
                return cacheItem;
            }

            cacheItem = SetCacheStatus(key);
            return cacheItem;
        }

        /// <summary>
        /// This is used to Set the date time of the cache tracker to either a given date, or the file last modified date
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="cacheInHours">Expiration in hours</param>
        /// <param name="dateModified">If this is null, it will assign the tracker to the file last modified date</param>
        /// <returns></returns>
        public static CacheTracker SetCacheStatus(string key, int cacheInHours = 24, DateTime? dateModified = null)
        {
            var fileName = $"{key}.json";
            var cacheItem = Cache.Contains(fileName) && Cache.Get(fileName) != null ? (CacheTracker)Cache[fileName] : null;

            if (cacheItem != null)
            {
                CacheUtility.Remove(fileName);
            }

            //need to check once more due to multiple requests into the lock
            cacheItem = Cache.Contains(fileName) && Cache.Get(fileName) != null ? (CacheTracker)Cache[fileName] : null;
            if (cacheItem == null)
            {

                if (dateModified == null)
                {
                    //update cache date modified from file
                    var fileInfo = GetCacheFileInfo(key);
                    var modified = DateTime.UtcNow;
                    if (fileInfo.Exists)
                    {
                        modified = fileInfo.LastWriteTimeUtc;
                    }

                    var cacheTracker = new CacheTracker
                    {
                        FileName = fileName,
                        Modified = modified
                    };
                    cacheItem = CacheUtility.Set<CacheTracker>(fileName, cacheTracker, cacheInHours);
                }
                else
                {
                    //update cache date modifed from passed in value
                    var cacheTracker = new CacheTracker
                    {
                        FileName = fileName,
                        Modified = (DateTime)dateModified
                    };
                    cacheItem = CacheUtility.Set<CacheTracker>(fileName, cacheTracker, cacheInHours);
                }

            }
            return cacheItem;
            
        }

        public static bool CacheHasUpdates(string key, bool updateCacheIfTrue = false)
        {
            var hasUpdates = false;

            var fileInfo = GetCacheFileInfo(key);
            var updatedDate = fileInfo.Exists ? fileInfo.LastWriteTimeUtc : DateTime.UtcNow;
            var tracker = GetCacheStatus(key);

            if (updatedDate > tracker.Modified)
            {
                if (updateCacheIfTrue)
                {
                    SetCacheStatus(key);  //update the cache
                }
                
                hasUpdates = true;
            }
            return hasUpdates;
        }

        public static CacheTracker SyncToFile<T>(string key, object value, object lockObject = null)
        {
            var cacheTracker = GetCacheStatus(key);
            var lastModified = cacheTracker.Modified;
            var fileInfo = GetCacheFileInfo(key);
            if (lastModified > fileInfo.LastWriteTimeUtc)
            {
                CacheByKey<T>(key, value);
            }
            return cacheTracker;
        }

        public static bool RemoveFilesStartsWith(string startsWith)
        {
            var folder = GetCacheFolder();
            foreach (var dataFile in folder.GetFiles().Where(i => i.Name.EndsWith(".json") && i.Name.StartsWith(startsWith)))
            {
                if (dataFile.Exists)
                {
                    dataFile.Delete();
                }
            }
            return false;
        }
    }
}
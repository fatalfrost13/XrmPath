using System;
using System.IO;
using Newtonsoft.Json;

namespace XrmPath.Helpers.Serializers
{
    public static class FileSerializer
    {

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileInfo"></param>
        /// <param name="result"></param>
        /// <returns>True if successful</returns>
        public static bool SerializeToFile<T>(FileInfo fileInfo, object result)
        {
            try
            {
                using (var streamWriter = File.CreateText(fileInfo.FullName))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(streamWriter, (T) result);
                }

                return true;
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning($"XrmPath.Helpers caught error on FileSerializer.SerializeToFile(): {ex}");
                //LogHelper.Warn<T>($"XrmPath.Helpers caught error on FileSerializer.SerializeToFile(): {ex}");
                return false;
            }
        }

        public static T DeserializeFromFile<T>(FileInfo fileInfo)
        {
            try
            {
                object deserializedObject = null;

                if (fileInfo.Exists)
                {
                    using (var sr = fileInfo.OpenText())
                    {
                        var serializer = new JsonSerializer();
                        deserializedObject = (T) serializer.Deserialize(sr, typeof(T));
                    }
                }

                return (T)deserializedObject;
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning($"XrmPath.Helpers caught error on FileSerializer.DeserializeFromFile(): {ex}");
                //LogHelper.Warn<T>($"XrmPath.Helpers caught error on FileSerializer.DeserializeFromFile(): {ex}");
                return default(T);
            }
        }
    }
}
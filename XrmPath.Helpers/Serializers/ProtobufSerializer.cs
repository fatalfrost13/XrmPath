using System;
using System.IO;
using ProtoBuf;

namespace XrmPath.Helpers.Serializers
{

    /// <summary>
    /// Only draw back to using this is that you cannot pass in a Generic type. You need to specify a specific type for the object you want to serialize/deserialize.
    /// Ojects need to be flagged with [ProtoContract(SkipConstructor = true)]  or [ProtoMember(1)]
    /// See https://www.c-sharpcorner.com/article/serialization-and-deserialization-ib-c-sharp-using-protobuf-dll/
    /// </summary>
    public static class ProtobufSerializer
    {
        public static byte[] SerializeBytes<T>(T record) where T: class
        {
            if (null == record) return null;

            try
            {
                using (var stream = new MemoryStream())
                {
                    Serializer.Serialize(stream, record);
                    return stream.ToArray();
                }
            }
            catch(Exception ex)
            {
                // Log error
                Serilog.Log.Warning($"XrmPath.Helpers caught error on ProtobufSerializer.SerializeBytes(): {ex}");
                //LogHelper.Warn<T>($"XrmPath.Helpers caught error on ProtobufSerializer.SerializeBytes(): {ex}");
                return null;
            }
        }

        public static string Serialize<T>(T record) where T : class
        {
            if (null == record) return null;

            try
            {
                using (var stream = new MemoryStream())
                {
                    Serializer.Serialize(stream, record);
                    return Convert.ToBase64String(stream.ToArray());
                    //return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                // Log error
                Serilog.Log.Warning($"XrmPath.Helpers caught error on ProtobufSerializer.Serialize(): {ex}");
                //LogHelper.Warn<T>($"XrmPath.Helpers caught error on ProtobufSerializer.Serialize(): {ex}");
                return null;
            }
        }

        public static T DeserializeBytes<T>(byte[] data) where T: class
        {
            if (null == data) return default(T);

            try
            {
                using (var stream = new MemoryStream(data))
                {
                    return Serializer.Deserialize<T>(stream);
                }
            }
            catch(Exception ex)
            {
                Serilog.Log.Warning($"XrmPath.Helpers caught error on ProtobufSerializer.DeserializeBytes(): {ex}");
                //LogHelper.Warn<T>($"XrmPath.Helpers caught error on ProtobufSerializer.DeserializeBytes(): {ex}");
                return default(T);
            }
        }

        public static T Deserialize<T>(string jsonData) where T : class
        {
            var data = !string.IsNullOrEmpty(jsonData) ? Convert.FromBase64String(jsonData) : null;

            if (null == data) return default(T);

            try
            {
                using (var stream = new MemoryStream(data))
                {
                    return Serializer.Deserialize<T>(stream);
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning($"XrmPath.Helpers caught error on ProtobufSerializer.Deserialize(): {ex}");
                //LogHelper.Warn<T>($"XrmPath.Helpers caught error on ProtobufSerializer.Deserialize(): {ex}");
                return default(T);
            }
        }
    }
}
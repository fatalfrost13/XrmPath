using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace XrmPath.Helpers.Serializers
{

    /// <summary>
    /// When using this serializer, objects need to be marked as serializable.
    /// </summary>
    public static class BinarySerializer
    {
        private static readonly BinaryFormatter Formatter = new BinaryFormatter();

        public static byte[] Serialize(object toSerialize)
        {
            using (var stream = new MemoryStream())
            {
                Formatter.Serialize(stream, toSerialize);
                return stream.ToArray();
            }
        }

        public static T Deserialize<T>(byte[] serialized)
        {
            using (var stream = new MemoryStream(serialized))
            {
                var result = (T)Formatter.Deserialize(stream);
                return result;
            }
        }
    }
}
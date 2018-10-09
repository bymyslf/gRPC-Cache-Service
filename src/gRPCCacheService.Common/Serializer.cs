using Newtonsoft.Json;
using System.Text;

namespace gRPCCacheService.Common
{
    public static class Serializer<T>
    {
        public static byte[] ToBytes(T obj)
        {
            var strObj = JsonConvert.SerializeObject(obj);
            return Encoding.UTF8.GetBytes(strObj);
        }

        public static T FromBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                return default(T);
            }

            var str = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(str);
        }
    }
}
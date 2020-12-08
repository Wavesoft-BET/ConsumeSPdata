using System.Text;
using Newtonsoft.Json;

namespace BLLFeedUpdates
{
    public static class ObjectSerializer
    {
        public static byte[] Serialize(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var json = JsonConvert.SerializeObject(obj);
            return Encoding.UTF8.GetBytes(json);
        }

        public static string Deserialize(byte[] buffer)
        {
            return Encoding.UTF8.GetString(buffer);
        }
    }
}

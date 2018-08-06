using Newtonsoft.Json;
using System.IO;

namespace Announcements.Helpers.Extensions
{
    public static class StreamExtensions
    {
        public static T GetDeserializedDataFromStream<T>(this Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            {
                var result = streamReader.ReadToEndAsync().Result;
                return JsonConvert.DeserializeObject<T>(result);
            }
        }
    }
}

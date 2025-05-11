using Newtonsoft.Json;

namespace Entities.Utilities
{
    public static class JsonUtility
    {
        public static string SerializeData<T>(T data)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Error
            };
            var returnValue = JsonConvert.SerializeObject(data, settings);

            return returnValue;
        }

        public static T DeserializeData<T>(string result)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Error;
            return JsonConvert.DeserializeObject<T>(result, settings);
        }
    }
}

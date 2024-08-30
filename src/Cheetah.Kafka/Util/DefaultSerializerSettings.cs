using Newtonsoft.Json;

namespace Cheetah.Kafka.Util
{
    /// <summary>
    /// Default serializer settings for JSON serialization
    /// </summary>
    public class DefaultSerializerSettings
    {
        /// <summary>
        /// Gets the default serializer settings
        /// </summary>
        /// <returns>The default serializer settings</returns>
        public static JsonSerializerSettings GetDefaultSettings()
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
            };

            return settings;
        }
    }
}
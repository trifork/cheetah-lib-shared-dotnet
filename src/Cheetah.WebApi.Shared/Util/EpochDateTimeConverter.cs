using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cheetah.Shared.WebApi.Util;

public class EpochDateTimeConverter : DateTimeConverterBase
{
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return DateTime.UnixEpoch.AddMilliseconds((long)reader.Value).ToLocalTime();
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var unixTimeMilliseconds = new DateTimeOffset((DateTime)value).ToUnixTimeMilliseconds();
        writer.WriteValue(unixTimeMilliseconds);
    }
}

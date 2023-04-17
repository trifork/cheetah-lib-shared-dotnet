using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cheetah.Shared.WebApi.Util;

public class EpochDateTimeConverter : DateTimeConverterBase
{
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        try
        {
            return reader.Value switch
            {
                long val => DateTime.UnixEpoch.AddMilliseconds(val).ToLocalTime(),
                string val => DateTime.TryParse(val, out var dateTime)
                    ? dateTime.ToLocalTime()
                    : throw new ArgumentException(
                        $"Attempted to deserialize the string '{val}', into a DateTime, but it could not be parsed"),
                _ => throw new ArgumentException(
                    $"Unable to deserialize type '{reader.Value?.GetType().Name}' into a DateTime.")
            };
        }
        catch (Exception e)
        {
            throw new JsonSerializationException($"Unable to deserialize '{reader.Value}' into a DateTime. See inner exception for details", e);
        }
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var unixTimeMilliseconds = new DateTimeOffset((DateTime)value).ToUnixTimeMilliseconds();
        writer.WriteValue(unixTimeMilliseconds);
    }
}

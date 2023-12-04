using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cheetah.Core.Util
{
    public class UtcDateTimeConverter : DateTimeConverterBase
    {
        public override object? ReadJson(
            JsonReader reader,
            Type objectType,
            object? existingValue,
            JsonSerializer serializer
        )
        {
            try
            {
                if(objectType == typeof(DateTime))
                {
                    return ReadDateTime(reader, objectType, existingValue, serializer);
                }

                if (objectType == typeof(DateTimeOffset))
                {
                    var dateTime = ReadDateTime(reader, objectType, existingValue, serializer);
                    return dateTime.HasValue
                        ? new DateTimeOffset(dateTime.Value)
                        : null;
                }
                
                throw new ArgumentException(
                    $"Cannot convert to requested object type: {objectType.FullName}. " +
                    $"Supported types are {typeof(DateTime).FullName} and {typeof(DateTimeOffset).FullName}");
            }
            catch (Exception e)
            {
                throw new JsonSerializationException(
                    $"Unable to deserialize '{reader.Value}' into a DateTime. See inner exception for details",
                    e
                );
            }
        }

        private static DateTime? ReadDateTime(JsonReader reader, Type objectType, object? existingValue,
            JsonSerializer serializer)
        {
            return reader.Value switch
            {
                long val => DateTime.UnixEpoch.AddMilliseconds(val).ToUniversalTime(),
                string val => DateTime.TryParse(val, out var dateTime)
                    ? dateTime.ToUniversalTime()
                    : throw new ArgumentException(
                        $"Attempted to deserialize the string '{val}', into a DateTime, but it could not be parsed"
                    ),
                DateTime val => val.ToUniversalTime(),
                DateTimeOffset val => new DateTime(val.ToUniversalTime().Ticks, DateTimeKind.Utc),
                null => null,
                _ => throw new ArgumentException(
                    $"Unable to deserialize type '{reader.Value?.GetType().FullName}' into a DateTime."
                )
            };
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            long? unixTimeMilliseconds = value switch
            {
                null => null,
                DateTime dt =>
                    dt == default
                        ? 0
                        : new DateTimeOffset(dt).ToUnixTimeMilliseconds(),
                DateTimeOffset dto =>
                    dto == default
                        ? 0
                        : dto.ToUnixTimeMilliseconds(),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(value), 
                    value, 
                    $"Unable to extract epoch millis from object of type {value.GetType().Name}")
            };
            writer.WriteValue(unixTimeMilliseconds);
        }
    }
}

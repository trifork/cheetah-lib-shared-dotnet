using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cheetah.Core.Util
{
    public class UtcDateTimeConverter : DateTimeConverterBase
    {
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object? existingValue,
            JsonSerializer serializer
        )
        {
            try
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
                    _ => throw new ArgumentException(
                            $"Unable to deserialize type '{reader.Value?.GetType().FullName}' into a DateTime."
                        )
                };
            }
            catch (Exception e)
            {
                throw new JsonSerializationException(
                    $"Unable to deserialize '{reader.Value}' into a DateTime. See inner exception for details",
                    e
                );
            }
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            long? unixTimeMilliseconds = value switch
            {
                DateTime dt => new DateTimeOffset(dt).ToUnixTimeMilliseconds(),
                DateTimeOffset dto => dto.ToUnixTimeMilliseconds(),
                null => null,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, $"Unable to extract epoch millis from object of type {value?.GetType().Name}")
            };
            writer.WriteValue(unixTimeMilliseconds);
        }
    }
}

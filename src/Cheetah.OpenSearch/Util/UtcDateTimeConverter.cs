using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cheetah.OpenSearch.Util
{
    /// <summary>
    /// Converts a <see cref="DateTime"/> to and from Unix epoch time (milliseconds)
    /// </summary>
    public class UtcDateTimeConverter : DateTimeConverterBase
    {
        /// <summary>
        /// Converts a <see cref="DateTime"/> to and from Unix epoch time (milliseconds)
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="JsonSerializationException"></exception>
        public override object? ReadJson(
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
                    null => null,
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
                null => null,
                DateTime dt =>
                    dt == default
                        ? 0
                        : new DateTimeOffset(dt).ToUnixTimeMilliseconds(),
                DateTimeOffset dto =>
                    dto == default
                        ? 0
                        : dto.ToUnixTimeMilliseconds(),
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, $"Unable to extract epoch millis from object of type {value?.GetType().Name}")
            };
            writer.WriteValue(unixTimeMilliseconds);
        }
    }
}

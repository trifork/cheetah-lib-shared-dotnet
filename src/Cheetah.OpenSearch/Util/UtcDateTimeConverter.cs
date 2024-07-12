using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cheetah.OpenSearch.Util
{
    /// <summary>
    /// Converts <see cref="DateTime"/>s and <see cref="DateTimeOffset"/> to and from Unix epoch time (milliseconds)
    /// </summary>
    /// <remarks>
    /// This converter is used to ensure that all <see cref="DateTime"/>s are serialized to Unix epoch time (milliseconds).<br/>
    /// <c>null</c> values are (de)serialized as <c>null</c>.
    /// </remarks>
    public class UtcDateTimeConverter : DateTimeConverterBase
    {
        /// <inheritdoc/>
        public override object? ReadJson(
            JsonReader reader,
            Type objectType,
            object? existingValue,
            JsonSerializer serializer
        )
        {
            try
            {
                if (objectType == typeof(DateTime))
                {
                    return ReadDateTime(reader);
                }

                if (objectType == typeof(DateTimeOffset))
                {
                    var dateTime = ReadDateTime(reader);
                    return dateTime.HasValue
                        ? new DateTimeOffset(dateTime.Value)
                        : null;
                }

                throw new ArgumentException(
                    $"Cannot convert to requested object type: {objectType.FullName}. "
                        + $"Supported types are {typeof(DateTime).FullName} and {typeof(DateTimeOffset).FullName}"
                );
            }
            catch (Exception e)
            {
                throw new JsonSerializationException(
                    $"Unable to deserialize '{reader.Value}' into a DateTime. See inner exception for details",
                    e
                );
            }
        }

        private static DateTime? ReadDateTime(
            JsonReader reader
        )
        {
            return reader.Value switch
            {
                long val => DateTime.UnixEpoch.AddMilliseconds(val).ToUniversalTime(),
                string val
                    => DateTime.TryParse(val, out var dateTime)
                        ? dateTime.ToUniversalTime()
                        : throw new ArgumentException(
                            $"Attempted to deserialize the string '{val}', into a DateTime, but it could not be parsed"
                        ),
                DateTime val => val.ToUniversalTime(),
                DateTimeOffset val => new DateTime(val.ToUniversalTime().Ticks, DateTimeKind.Utc),
                null => null,
                _
                    => throw new ArgumentException(
                        $"Unable to deserialize type '{reader.Value?.GetType().FullName}' into a DateTime."
                    )
            };
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            long? unixTimeMilliseconds = value switch
            {
                null => null,
                DateTime dt => dt == default ? 0 : new DateTimeOffset(dt).ToUnixTimeMilliseconds(),
                DateTimeOffset dto => dto == default ? 0 : dto.ToUnixTimeMilliseconds(),
                _
                    => throw new ArgumentOutOfRangeException(
                        nameof(value),
                        value,
                        $"Unable to extract epoch millis from object of type {value.GetType().Name}"
                    )
            };
            writer.WriteValue(unixTimeMilliseconds);
        }
    }
}

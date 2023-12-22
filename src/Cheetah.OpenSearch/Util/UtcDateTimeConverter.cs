using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cheetah.Core.Util
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
        /// <summary>
        /// Reads a <see cref="DateTime"/> from Unix epoch time (milliseconds)
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from </param>
        /// <param name="objectType">The type of the object being read.</param>
        /// <param name="existingValue">The existing value of object being read. If there is no existing value then null will be used.</param>
        /// <param name="serializer">The calling serializer. If you need to call <see cref="JsonSerializer"/> methods on this object, use this.</param>
        /// <returns>A <see cref="DateTime"/> populated from the reader.</returns>
        /// <exception cref="JsonSerializationException">Thrown if the value read from the reader cannot be converted into a <see cref="DateTime"/>.</exception>
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

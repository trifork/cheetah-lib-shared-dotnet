using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cheetah.WebApi.Shared.Util;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Cheetah.WebApi.Shared.Test.Util
{
    [Trait("Category", "Utils"), Trait("TestType", "Unit")]
    public class UtcDateTimeConverterTest
    {
        private readonly UtcDateTimeConverter _sut;

        public UtcDateTimeConverterTest()
        {
            _sut = new UtcDateTimeConverter();
        }

        public static IEnumerable<object[]> ValidTestCases =>
            new List<object[]>
            {
                new object[] { "0", DateTime.UnixEpoch },
                new object[] { "123", DateTime.UnixEpoch.AddMilliseconds(123) },
                new object[]
                {
                    "2147483647001",
                    DateTime.UnixEpoch.AddSeconds(int.MaxValue).AddMilliseconds(1)
                }, // Survives Y2K38
                new object[] { "\"1970-01-01 00:00:00Z\"", DateTime.UnixEpoch },
                new object[] { "\"1970-01-01 01:00:00+0100\"", DateTime.UnixEpoch },
                new object[]
                {
                    "\"1970-01-01 00:00:00.123Z\"",
                    DateTime.UnixEpoch.AddMilliseconds(123)
                },
                new object[]
                {
                    "\"2038-01-19 03:14:07.001Z\"",
                    DateTime.UnixEpoch.AddSeconds(int.MaxValue).AddMilliseconds(1)
                } // Survives Y2K38
            };

        [Theory]
        [MemberData(nameof(ValidTestCases))]
        public void Should_CorrectlyConvertJsonToDateTime_When_ProvidedValidDatetimeJson(
            string json,
            DateTime expected
        )
        {
            var reader = new JsonTextReader(new StringReader(json));
            while (reader.TokenType == JsonToken.None)
            {
                if (!reader.Read())
                    break;
            }

            var actual = (DateTime)
                _sut.ReadJson(reader, typeof(DateTime), null, JsonSerializer.CreateDefault());

            Assert.Equal(expected, actual);
        }

        public record DummyDateTime(DateTime DateTime);

        [Theory]
        [MemberData(nameof(ValidTestCases))]
        public void Should_CorrectlyDeserializeDateTimeRepresentations_When_UsedInASerializer(
            string valueJson,
            DateTime expected
        )
        {
            var json = $"{{ \"DateTime\": {valueJson} }}";
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(_sut);
            var actual = JsonConvert.DeserializeObject<DummyDateTime>(json, settings);

            Assert.Equal(expected, actual?.DateTime);
        }

        [Theory]
        [InlineData("")]
        [InlineData("{}")]
        [InlineData("{\"someValue\":\"0\"")]
        [InlineData("[0]")]
        [InlineData("\"ThisIsNotADateTime\"")]
        public void Should_ThrowJsonSerializationException_When_ProvidedInvalidDatetimeJson(
            string json
        )
        {
            var reader = new JsonTextReader(new StringReader(json));
            while (reader.TokenType == JsonToken.None)
            {
                if (!reader.Read())
                    break;
            }

            Assert.Throws<JsonSerializationException>(
                () => _sut.ReadJson(reader, typeof(DateTime), null, JsonSerializer.CreateDefault())
            );
        }

        [Fact]
        public void Should_CorrectlyReadDateTime()
        {
            DateTime dateTime = DateTime.UnixEpoch;
            var readerMock = new Mock<JsonReader>();
            readerMock.SetupGet(x => x.Value).Returns(dateTime);

            var value = (DateTime)
                _sut.ReadJson(
                    readerMock.Object,
                    typeof(DateTime),
                    null,
                    JsonSerializer.CreateDefault()
                );

            Assert.Equal(dateTime, value);
        }

        [Fact]
        public void Should_CorrectlyReadDateTimeOffset()
        {
            var dateTime = DateTimeOffset.UnixEpoch;
            var readerMock = new Mock<JsonReader>();
            readerMock.SetupGet(x => x.Value).Returns(dateTime);

            var value = (DateTime)
                _sut.ReadJson(
                    readerMock.Object,
                    typeof(DateTime),
                    null,
                    JsonSerializer.CreateDefault()
                );

            Assert.Equal(dateTime, value);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData((long)int.MaxValue + 1)] // Survives Y2K38
        public void Should_CorrectlySerializeDateTimeIntoEpochMillis_When_ProvidedValidDateTime(
            long epochSeconds
        )
        {
            var dateTime = DateTime.UnixEpoch.AddSeconds(epochSeconds);

            var sb = new StringBuilder();
            var writer = new JsonTextWriter(new StringWriter(sb));
            _sut.WriteJson(writer, dateTime, JsonSerializer.CreateDefault());

            var value = sb.ToString();

            Assert.Equal(value, (epochSeconds * 1000).ToString());
        }
    }
}

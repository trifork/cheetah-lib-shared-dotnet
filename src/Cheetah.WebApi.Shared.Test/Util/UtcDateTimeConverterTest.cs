using System;
using System.IO;
using System.Text;
using Cheetah.Core.Util;
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

        public static TheoryData<string, DateTime> ValidDateTimeTestCases =>
            new TheoryData<string, DateTime>
            {
                { "-2147483647001", DateTime.UnixEpoch.AddSeconds(-int.MaxValue).AddMilliseconds(-1) }, // Survives Y2K38
                { "-123", DateTime.UnixEpoch.AddMilliseconds(-123) },
                { "0", DateTime.UnixEpoch },
                { "123", DateTime.UnixEpoch.AddMilliseconds(123) },
                { "\"1970-01-01 00:00:00.123Z\"", DateTime.UnixEpoch.AddMilliseconds(123) },
                { "2147483647001", DateTime.UnixEpoch.AddSeconds(int.MaxValue).AddMilliseconds(1) }, // Survives Y2K38
                { "\"1970-01-01 00:00:00Z\"", DateTime.UnixEpoch },
                { "\"1970-01-01 01:00:00+0100\"", DateTime.UnixEpoch },
                { "\"1970-01-01T01:00:00+0100\"", DateTime.UnixEpoch },
                { "\"1970-01-01T01:00:00Z\"", DateTime.UnixEpoch.AddHours(1) },
                { "\"1969-01-01T00:00:00Z\"", DateTime.UnixEpoch.AddYears(-1) },
                { "\"970-01-01T00:00:00Z\"", DateTime.UnixEpoch.AddYears(-1000) },
                { "\"2038-01-19 03:14:07.001Z\"", DateTime.UnixEpoch.AddSeconds(int.MaxValue).AddMilliseconds(1) } // Survives Y2K38
            };

        [Theory]
        [MemberData(nameof(ValidDateTimeTestCases))]
        public void Should_ConvertJsonToDateTime_When_ProvidedValidDatetimeJson(
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

            var actual = (DateTime?)_sut.ReadJson(reader, typeof(DateTime), null, JsonSerializer.CreateDefault());

            Assert.Equal(expected, actual);
        }

        public record DummyDateTime(DateTime DateTime);

        [Theory]
        [MemberData(nameof(ValidDateTimeTestCases))]
        public void Should_DeserializeDateTimeRepresentations_When_UsedInASerializer(string valueJson, DateTime expected)
        {
            var json = $"{{ \"DateTime\": {valueJson} }}";
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(_sut);
            var actual = JsonConvert.DeserializeObject<DummyDateTime>(json, settings);

            Assert.Equal(expected, actual?.DateTime);
        }

        public static TheoryData<string, DateTimeOffset> ValidDateTimeOffsetTestCases =>
            new TheoryData<string, DateTimeOffset>
            {
                { "-2147483647001", DateTimeOffset.UnixEpoch.AddSeconds(-int.MaxValue).AddMilliseconds(-1) }, // Survives Y2K38
                { "-123", DateTimeOffset.UnixEpoch.AddMilliseconds(-123) },
                { "0", DateTimeOffset.UnixEpoch },
                { "123", DateTimeOffset.UnixEpoch.AddMilliseconds(123) },
                { "\"1970-01-01 00:00:00.123Z\"", DateTimeOffset.UnixEpoch.AddMilliseconds(123) },
                { "2147483647001", DateTimeOffset.UnixEpoch.AddSeconds(int.MaxValue).AddMilliseconds(1) }, // Survives Y2K38
                { "\"1970-01-01 00:00:00Z\"", DateTimeOffset.UnixEpoch },
                { "\"1970-01-01 01:00:00+0100\"", DateTimeOffset.UnixEpoch },
                { "\"1970-01-01T01:00:00+0100\"", DateTimeOffset.UnixEpoch },
                { "\"1970-01-01T01:00:00Z\"", DateTimeOffset.UnixEpoch.AddHours(1) },
                { "\"1969-01-01T00:00:00Z\"", DateTimeOffset.UnixEpoch.AddYears(-1) },
                { "\"970-01-01T00:00:00Z\"", DateTimeOffset.UnixEpoch.AddYears(-1000) },
                { "\"2038-01-19 03:14:07.001Z\"", DateTimeOffset.UnixEpoch.AddSeconds(int.MaxValue).AddMilliseconds(1) } // Survives Y2K38
            };

        [Theory]
        [MemberData(nameof(ValidDateTimeOffsetTestCases))]
        public void Should_ConvertJsonToDateTimeOffset_When_ProvidedValidDatetimeJson(
            string json,
            DateTimeOffset expected
        )
        {
            var reader = new JsonTextReader(new StringReader(json));
            while (reader.TokenType == JsonToken.None)
            {
                if (!reader.Read())
                    break;
            }

            var actual = (DateTimeOffset?)_sut.ReadJson(reader, typeof(DateTimeOffset), null, JsonSerializer.CreateDefault());

            Assert.Equal(expected, actual);
        }

        public record DummyDateTimeOffset(DateTimeOffset DateTimeOffset);

        [Theory]
        [MemberData(nameof(ValidDateTimeOffsetTestCases))]
        public void Should_DeserializeDateTimeOffsetRepresentations_When_UsedInASerializer(string valueJson, DateTimeOffset expected)
        {
            var json = $"{{ \"DateTimeOffset\": {valueJson} }}";
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(_sut);
            var actual = JsonConvert.DeserializeObject<DummyDateTimeOffset>(json, settings);

            Assert.Equal(expected, actual?.DateTimeOffset);
        }

        [Theory]
        [InlineData("\"12345-1-123Q2:409:0.A!100:002\"")]
        [InlineData("\"ThisIsNotADateTime\"")]
        public void Should_ThrowJsonSerializationException_When_ProvidedInvalidDatetimeJson(string json)
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
        public void Should_ReadDateTime()
        {
            DateTime dateTime = DateTime.UnixEpoch;
            var readerMock = new Mock<JsonReader>();
            readerMock.SetupGet(x => x.Value).Returns(dateTime);

            var value = (DateTime?)
                _sut.ReadJson(
                    readerMock.Object,
                    typeof(DateTime),
                    null,
                    JsonSerializer.CreateDefault()
                );

            Assert.Equal(dateTime, value);
        }

        [Fact]
        public void Should_ReadDateTimeOffset()
        {
            DateTimeOffset? dateTime = DateTimeOffset.UnixEpoch;
            var readerMock = new Mock<JsonReader>();
            readerMock.SetupGet(x => x.Value).Returns(dateTime);

            var value = (DateTimeOffset?)
                _sut.ReadJson(
                    readerMock.Object,
                    typeof(DateTimeOffset),
                    null,
                    JsonSerializer.CreateDefault()
                );

            Assert.Equal(dateTime, value);
        }

        [Theory]
        [InlineData((long)int.MinValue - 1)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData((long)int.MaxValue + 1)] // Survives Y2K38
        public void Should_SerializeDateTimeIntoEpochMillis_When_ProvidedValidDateTime(
            long epochSeconds
        )
        {
            var dateTime = DateTime.UnixEpoch.AddSeconds(epochSeconds);

            var sb = new StringBuilder();
            var writer = new JsonTextWriter(new StringWriter(sb));
            _sut.WriteJson(writer, dateTime, JsonSerializer.CreateDefault());

            Assert.Equal((epochSeconds * 1000).ToString(), sb.ToString());
        }

        [Theory]
        [InlineData((long)int.MinValue - 1)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData((long)int.MaxValue + 1)]
        public void Should_WriteDateTimeOffsets(long epochSeconds)
        {
            var date = DateTimeOffset.UnixEpoch.AddSeconds(epochSeconds);

            var sb = new StringBuilder();
            var writer = new JsonTextWriter(new StringWriter(sb));
            _sut.WriteJson(writer, date, JsonSerializer.CreateDefault());

            Assert.Equal((epochSeconds * 1000).ToString(), sb.ToString());
        }

        [Fact]
        public void Should_WriteNullAsNull()
        {
            object? date = null;
            var sb = new StringBuilder();
            var writer = new JsonTextWriter(new StringWriter(sb));
            _sut.WriteJson(writer, date, JsonSerializer.CreateDefault());

            Assert.Equal("null", sb.ToString());
        }

        [Fact]
        public void Should_SerializeDefaultDateTimeAs0()
        {
            DateTime date = new();
            var sb = new StringBuilder();
            var writer = new JsonTextWriter(new StringWriter(sb));
            _sut.WriteJson(writer, date, JsonSerializer.CreateDefault());

            Assert.Equal("0", sb.ToString());
        }

        [Fact]
        public void Should_SerializeDefaultDateTimeOffsetAs0()
        {
            DateTimeOffset date = new();
            var sb = new StringBuilder();
            var writer = new JsonTextWriter(new StringWriter(sb));
            _sut.WriteJson(writer, date, JsonSerializer.CreateDefault());

            Assert.Equal("0", sb.ToString());
        }
    }
}

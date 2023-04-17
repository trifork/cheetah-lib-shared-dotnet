using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cheetah.Shared.WebApi.Util;
using Newtonsoft.Json;
using Xunit;

namespace Cheetah.WebApi.Shared_test.Util;

[Trait("Category", "Utils"), Trait("TestType", "Unit")]
public class EpochDateTimeConverterTest
{
    
    private EpochDateTimeConverter _sut;
    public EpochDateTimeConverterTest()
    {
        _sut = new EpochDateTimeConverter();
    }


    public static IEnumerable<object[]> ValidTestCases => new List<object[]>
    {
        new object[] { "0", DateTime.UnixEpoch.ToLocalTime() },
        new object[] { "123", DateTime.UnixEpoch.AddMilliseconds(123).ToLocalTime() },
        new object[] { "2147483647001", DateTime.UnixEpoch.AddSeconds(int.MaxValue).AddMilliseconds(1).ToLocalTime() }, // Survives Y2K38
        new object[] { "\"1970-01-01 00:00:00Z\"", DateTime.UnixEpoch.ToLocalTime() },
        new object[] { "\"1970-01-01 01:00:00+0100\"", DateTime.UnixEpoch.ToLocalTime() },
        new object[] { "\"1970-01-01 00:00:00.123\"", DateTime.UnixEpoch.AddMilliseconds(123).ToLocalTime() },
        new object[] { "\"2038-01-19 03:14:07.001Z\"", DateTime.UnixEpoch.AddSeconds(int.MaxValue).AddMilliseconds(1).ToLocalTime() } // Survives Y2K38
    };

    [Theory]
    [MemberData(nameof(ValidTestCases))]
    public void Should_CorrectlyConvertJsonToDateTime_When_ProvidedValidDatetimeJson(string json, DateTime expected)
    {
        var reader = new JsonTextReader(new StringReader(json));
        while (reader.TokenType == JsonToken.None)
            if (!reader.Read())
                break;

        var obj = (DateTime) _sut.ReadJson(reader, typeof(DateTime), null, Newtonsoft.Json.JsonSerializer.CreateDefault());

        Assert.Equal(obj, expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{}")]
    [InlineData("{\"someValue\":\"0\"")]
    [InlineData("[0]")]
    [InlineData("\"ThisIsNotADateTime\"")]
    public void Should_ThrowJsonSerializationException_When_ProvidedInvalidDatetimeJson(string json)
    {
        var reader = new JsonTextReader(new StringReader(json));
        while (reader.TokenType == JsonToken.None)
            if (!reader.Read())
                break;

        Assert.Throws<JsonSerializationException>(() => _sut.ReadJson(reader, typeof(DateTime), null, Newtonsoft.Json.JsonSerializer.CreateDefault()));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData((long) int.MaxValue + 1)] // Survives Y2K38
    public void Should_CorrectlySerializeDateTimeIntoEpochMillis_When_ProvidedValidDateTime(long epochSeconds)
    {
        var dateTime = DateTime.UnixEpoch.AddSeconds(epochSeconds);

        var sb = new StringBuilder();
        var writer = new JsonTextWriter(new StringWriter(sb));
        _sut.WriteJson(writer, dateTime, Newtonsoft.Json.JsonSerializer.CreateDefault());

        var value = sb.ToString();
        
        Assert.Equal(value, (epochSeconds * 1000).ToString());
    }
}
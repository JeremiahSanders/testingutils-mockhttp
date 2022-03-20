using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Xunit;

namespace Jds.TestingUtils.MockHttp.Tests.Unit;

public class CapturedHttpRequestMessageTests
{
  [Fact]
  public async Task GivenNoContent_ParsesMessage()
  {
    var sourceMessage = MessageGenerators.NoContent();

    var captured = await CapturedHttpRequestMessage.FromHttpRequestMessage(sourceMessage);

    Assert.Equal(sourceMessage.Method, captured.Method);
    Assert.Null(captured.ContentBytes);
    Assert.Null(captured.ContentString);
  }
}

public static class MessageGenerators
{
  public static HttpRequestMessage NoContent()
  {
    return new HttpRequestMessage(HttpMethod.Post, $"https://{Guid.NewGuid()}")
    {
      Headers =
      {
        Accept =
        {
          new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json),
          new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Xml)
        },
        UserAgent = { new ProductInfoHeaderValue("Some.Name", "2.1.3") }
      }
    };
  }

  public static HttpRequestMessage JsonMessage()
  {
    return new HttpRequestMessage(HttpMethod.Post, $"https://{Guid.NewGuid()}")
    {
      Content = new DummyData { Value = Guid.NewGuid().ToString("D") }.ToJsonHttpContent(),
      Headers =
      {
        Accept = { new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json) },
        UserAgent = { new ProductInfoHeaderValue("Some.Name", "2.1.3") }
      }
    };
  }

  public static HttpRequestMessage XmlMessage()
  {
    var dummyData = new DummyData { Value = Guid.NewGuid().ToString("D") };
    var serializer = new XmlSerializer(typeof(DummyData));
    using var write = new StringWriter();
    serializer.Serialize(write, dummyData);

    return new HttpRequestMessage(HttpMethod.Post, $"https://{Guid.NewGuid()}")
    {
      Content = new StringContent(write.ToString(), Encoding.UTF8),
      Headers =
      {
        Accept = { new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Xml) },
        UserAgent = { new ProductInfoHeaderValue("Some.Name", "2.1.3") }
      }
    };
  }

  public record DummyData
  {
    [JsonPropertyName("value")]
    public string Value { get; init; } = string.Empty;
  }
}

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using Xunit;

namespace Jds.TestingUtils.MockHttp.Tests.Unit.MessageCaseHandlerBuilderExtensionsTests;

public class RespondDerivedContentXmlTests
{
  public class MessageAsyncFunc
  {
    [Fact]
    public async Task ReturnsArrangedResponse()
    {
      var responseSerializer = XmlSerialization.GetSerializer(typeof(FauxResponse));
      var caseHandler = new MessageCaseHandlerBuilder()
        .AcceptAll()
        .RespondDerivedContentXml(
          (_, _) => Task.FromResult(HttpStatusCode.OK),
          (message, token) => Task.FromResult(new FauxResponse { httpMethod = message.Method.Method })
        )
        .Build();
      var requestMessage = new HttpRequestMessage(HttpMethod.Patch, "https://not-real");
      var capturedRequest = await CapturedHttpRequestMessage.FromHttpRequestMessage(requestMessage);
      var expected = new FauxResponse { httpMethod = requestMessage.Method.Method };

      var response = await caseHandler.HandleMessage(capturedRequest, CancellationToken.None);
      var actual = (FauxResponse?)responseSerializer.Deserialize(await response.Content.ReadAsStreamAsync()) ??
                   new FauxResponse();

      Assert.Equal(expected, actual);
    }
  }

  public class ObjectAsyncFunc
  {
    [Fact]
    public async Task ReturnsArrangedResponse()
    {
      var responseSerializer = XmlSerialization.GetSerializer(typeof(FauxResponse));
      var caseHandler = new MessageCaseHandlerBuilder()
        .AcceptAll()
        .RespondDerivedContentXml(
          (_, _) => Task.FromResult(HttpStatusCode.OK),
          (request, token) => Task.FromResult(new FauxResponse { httpMethod = request.number.ToString() }),
          new FauxRequest()
        )
        .Build();
      var requestObject = new FauxRequest { number = 42 };
      var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://not-real")
      {
        Content = CreateHttpContent.ApplicationXml(requestObject)
      };
      var capturedRequest = await CapturedHttpRequestMessage.FromHttpRequestMessage(requestMessage);
      var expected = new FauxResponse { httpMethod = requestObject.number.ToString() };

      var response = await caseHandler.HandleMessage(capturedRequest, CancellationToken.None);
      var actual = (FauxResponse?)responseSerializer.Deserialize(await response.Content.ReadAsStreamAsync()) ??
                   new FauxResponse();

      Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GivenNoContent_UsesDefaultObject()
    {
      var responseSerializer = XmlSerialization.GetSerializer(typeof(FauxResponse));
      var defaultValue = 12;
      var caseHandler = new MessageCaseHandlerBuilder()
        .AcceptAll()
        .RespondDerivedContentXml(
          (_, _) => Task.FromResult(HttpStatusCode.OK),
          (request, token) => Task.FromResult(new FauxResponse { httpMethod = request.number.ToString() }),
          new FauxRequest { number = defaultValue }
        )
        .Build();
      var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://not-real");
      var capturedRequest = await CapturedHttpRequestMessage.FromHttpRequestMessage(requestMessage);
      var expected = new FauxResponse { httpMethod = defaultValue.ToString() };

      var response = await caseHandler.HandleMessage(capturedRequest, CancellationToken.None);
      var actual = (FauxResponse?)responseSerializer.Deserialize(await response.Content.ReadAsStreamAsync()) ??
                   new FauxResponse();

      Assert.Equal(expected, actual);
    }
  }

  public class ObjectFunc
  {
    [Fact]
    public async Task ReturnsArrangedResponse()
    {
      var responseSerializer = XmlSerialization.GetSerializer(typeof(FauxResponse));
      var caseHandler = new MessageCaseHandlerBuilder()
        .AcceptAll()
        .RespondDerivedContentXml(
          _ => HttpStatusCode.OK,
          request => new FauxResponse { httpMethod = request.number.ToString() },
          new FauxRequest()
        )
        .Build();
      var requestObject = new FauxRequest { number = 42 };
      var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://not-real")
      {
        Content = CreateHttpContent.ApplicationXml(requestObject)
      };
      var capturedRequest = await CapturedHttpRequestMessage.FromHttpRequestMessage(requestMessage);
      var expected = new FauxResponse { httpMethod = requestObject.number.ToString() };

      var response = await caseHandler.HandleMessage(capturedRequest, CancellationToken.None);
      var actual = (FauxResponse?)responseSerializer.Deserialize(await response.Content.ReadAsStreamAsync()) ??
                   new FauxResponse();

      Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GivenNoContent_UsesDefaultObject()
    {
      var responseSerializer = XmlSerialization.GetSerializer(typeof(FauxResponse));
      var defaultValue = 12;
      var caseHandler = new MessageCaseHandlerBuilder()
        .AcceptAll()
        .RespondDerivedContentXml(
          _ => HttpStatusCode.OK,
          request => new FauxResponse { httpMethod = request.number.ToString() },
          new FauxRequest { number = defaultValue }
        )
        .Build();
      var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://not-real");
      var capturedRequest = await CapturedHttpRequestMessage.FromHttpRequestMessage(requestMessage);
      var expected = new FauxResponse { httpMethod = defaultValue.ToString() };

      var response = await caseHandler.HandleMessage(capturedRequest, CancellationToken.None);
      var actual = (FauxResponse?)responseSerializer.Deserialize(await response.Content.ReadAsStreamAsync()) ??
                   new FauxResponse();

      Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GivenDifferingContent_UsesDefaultObject()
    {
      var responseSerializer = XmlSerialization.GetSerializer(typeof(FauxResponse));
      var defaultValue = 12;
      var caseHandler = new MessageCaseHandlerBuilder()
        .AcceptAll()
        .RespondDerivedContentXml(
          _ => HttpStatusCode.OK,
          request => new FauxResponse { httpMethod = request.number.ToString() },
          new FauxRequest { number = defaultValue }
        )
        .Build();
      var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://not-real")
      {
        Content = new StringContent("not it", Encoding.UTF8)
      };
      var capturedRequest = await CapturedHttpRequestMessage.FromHttpRequestMessage(requestMessage);
      var expected = new FauxResponse { httpMethod = defaultValue.ToString() };

      var response = await caseHandler.HandleMessage(capturedRequest, CancellationToken.None);
      var actual = (FauxResponse?)responseSerializer.Deserialize(await response.Content.ReadAsStreamAsync()) ??
                   new FauxResponse();

      Assert.Equal(expected, actual);
    }
  }

  [SuppressMessage("ReSharper", "MemberCanBePrivate.Global",
    Justification = "Private types cannot be serialized by XmlSerializer")]
  public record FauxResponse
  {
    public string httpMethod { get; init; } = string.Empty;
  }

  [SuppressMessage("ReSharper", "MemberCanBePrivate.Global",
    Justification = "Private types cannot be serialized by XmlSerializer")]
  public record FauxRequest
  {
    public int number { get; init; }
  }
}

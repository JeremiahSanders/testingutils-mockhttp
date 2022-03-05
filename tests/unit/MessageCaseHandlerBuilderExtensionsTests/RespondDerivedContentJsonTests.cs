using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Jds.TestingUtils.MockHttp.Tests.Unit.MessageCaseHandlerBuilderExtensionsTests;

public class RespondDerivedContentJsonTests
{
  public class MessageAsyncFunc
  {
    [Fact]
    public async Task ReturnsArrangedResponse()
    {
      var caseHandler = new MessageCaseHandlerBuilder()
        .AcceptAll()
        .RespondDerivedContentJson(
          HttpStatusCode.OK,
          (message, token) => Task.FromResult(new FauxResponse { httpMethod = message.Method.Method })
        )
        .Build();
      var requestMessage = new HttpRequestMessage(HttpMethod.Patch, "https://not-real");
      var expected = new FauxResponse { httpMethod = requestMessage.Method.Method };

      var response = await caseHandler.HandleMessage(requestMessage, CancellationToken.None);
      var actual = await response.Content.ReadFromJsonAsync<FauxResponse>();

      Assert.Equal(expected, actual);
    }
  }

  public class ObjectAsyncFunc
  {
    [Fact]
    public async Task ReturnsArrangedResponse()
    {
      var caseHandler = new MessageCaseHandlerBuilder()
        .AcceptAll()
        .RespondDerivedContentJson(
          HttpStatusCode.OK,
          (request, token) => Task.FromResult(new FauxResponse { httpMethod = request.number.ToString() }),
          new FauxRequest()
        )
        .Build();
      var requestObject = new FauxRequest { number = 42 };
      var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://not-real")
      {
        Content = new StringContent(JsonSerializer.Serialize(requestObject), Encoding.UTF8,
          MediaTypeNames.Application.Json)
      };
      var expected = new FauxResponse { httpMethod = requestObject.number.ToString() };

      var response = await caseHandler.HandleMessage(requestMessage, CancellationToken.None);
      var actual = await response.Content.ReadFromJsonAsync<FauxResponse>();

      Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GivenNoContent_UsesDefaultObject()
    {
      var defaultValue = 12;
      var caseHandler = new MessageCaseHandlerBuilder()
        .AcceptAll()
        .RespondDerivedContentJson(
          HttpStatusCode.OK,
          (request, token) => Task.FromResult(new FauxResponse { httpMethod = request.number.ToString() }),
          new FauxRequest { number = defaultValue }
        )
        .Build();
      var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://not-real");
      var expected = new FauxResponse { httpMethod = defaultValue.ToString() };

      var response = await caseHandler.HandleMessage(requestMessage, CancellationToken.None);
      var actual = await response.Content.ReadFromJsonAsync<FauxResponse>();

      Assert.Equal(expected, actual);
    }
  }

  public class ObjectFunc
  {
    [Fact]
    public async Task ReturnsArrangedResponse()
    {
      var caseHandler = new MessageCaseHandlerBuilder()
        .AcceptAll()
        .RespondDerivedContentJson(
          HttpStatusCode.OK,
          request => new FauxResponse { httpMethod = request.number.ToString() },
          new FauxRequest()
        )
        .Build();
      var requestObject = new FauxRequest { number = 42 };
      var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://not-real")
      {
        Content = new StringContent(JsonSerializer.Serialize(requestObject), Encoding.UTF8,
          MediaTypeNames.Application.Json)
      };
      var expected = new FauxResponse { httpMethod = requestObject.number.ToString() };

      var response = await caseHandler.HandleMessage(requestMessage, CancellationToken.None);
      var actual = await response.Content.ReadFromJsonAsync<FauxResponse>();

      Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GivenNoContent_UsesDefaultObject()
    {
      var defaultValue = 12;
      var caseHandler = new MessageCaseHandlerBuilder()
        .AcceptAll()
        .RespondDerivedContentJson(
          HttpStatusCode.OK,
          request => new FauxResponse { httpMethod = request.number.ToString() },
          new FauxRequest { number = defaultValue }
        )
        .Build();
      var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://not-real");
      var expected = new FauxResponse { httpMethod = defaultValue.ToString() };

      var response = await caseHandler.HandleMessage(requestMessage, CancellationToken.None);
      var actual = await response.Content.ReadFromJsonAsync<FauxResponse>();

      Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GivenDifferingContent_UsesDefaultObject()
    {
      var defaultValue = 12;
      var caseHandler = new MessageCaseHandlerBuilder()
        .AcceptAll()
        .RespondDerivedContentJson(
          HttpStatusCode.OK,
          request => new FauxResponse { httpMethod = request.number.ToString() },
          new FauxRequest { number = defaultValue }
        )
        .Build();
      var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://not-real")
      {
        Content = new StringContent("not it", Encoding.UTF8)
      };
      var expected = new FauxResponse { httpMethod = defaultValue.ToString() };

      var response = await caseHandler.HandleMessage(requestMessage, CancellationToken.None);
      var actual = await response.Content.ReadFromJsonAsync<FauxResponse>();

      Assert.Equal(expected, actual);
    }
  }

  private record FauxResponse
  {
    public string httpMethod { get; init; } = string.Empty;
  }

  private record FauxRequest
  {
    public int number { get; init; }
  }
}

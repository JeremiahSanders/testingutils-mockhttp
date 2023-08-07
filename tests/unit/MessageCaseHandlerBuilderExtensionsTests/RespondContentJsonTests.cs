using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Jds.TestingUtils.MockHttp.Tests.Unit.MessageCaseHandlerBuilderExtensionsTests;

public class RespondContentJsonTests
{
  [Fact]
  public async Task ReturnsArrangedResponse()
  {
    var value = Guid.NewGuid().ToString();
    var caseHandler = new MessageCaseHandlerBuilder()
      .AcceptAll()
      .RespondContentJson(
        HttpStatusCode.OK,
        new FauxResponse {value = value}
      )
      .Build();
    var requestMessage = new HttpRequestMessage(HttpMethod.Patch, "https://not-real");
    var capturedRequest = await CapturedHttpRequestMessage.FromHttpRequestMessage(requestMessage);
    var expected = new FauxResponse {value = value};

    var response = await caseHandler.HandleMessage(capturedRequest, CancellationToken.None);
    var actual = await response.Content.ReadFromJsonAsync<FauxResponse>();

    actual.Should().BeEquivalentTo(expected);
  }

  [Fact]
  public async Task ReturnsOriginalRequest()
  {
    var value = Guid.NewGuid().ToString();
    var caseHandler = new MessageCaseHandlerBuilder()
      .AcceptAll()
      .RespondContentJson(
        HttpStatusCode.OK,
        new FauxResponse {value = value}
      )
      .Build();
    var requestMessage = new HttpRequestMessage(HttpMethod.Patch, "https://not-real");
    var capturedRequest = await CapturedHttpRequestMessage.FromHttpRequestMessage(requestMessage);

    var response = await caseHandler.HandleMessage(capturedRequest, CancellationToken.None);

    response.RequestMessage.Should().BeEquivalentTo(requestMessage);
  }

  private record FauxResponse
  {
    public string value { get; init; } = string.Empty;
  }
}

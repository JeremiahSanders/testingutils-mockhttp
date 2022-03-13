using System.Net;
using System.Net.Http.Json;
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
        new FauxResponse { value = value }
      )
      .Build();
    var requestMessage = new HttpRequestMessage(HttpMethod.Patch, "https://not-real");
    var capturedRequest = await CapturedHttpRequestMessage.FromHttpRequestMessage(requestMessage);
    var expected = new FauxResponse { value = value };

    var response = await caseHandler.HandleMessage(capturedRequest, CancellationToken.None);
    var actual = await response.Content.ReadFromJsonAsync<FauxResponse>();

    Assert.Equal(expected, actual);
  }

  private record FauxResponse
  {
    public string value { get; init; } = string.Empty;
  }
}

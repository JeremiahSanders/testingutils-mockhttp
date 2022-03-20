using System.Diagnostics.CodeAnalysis;
using System.Net;
using Xunit;

namespace Jds.TestingUtils.MockHttp.Tests.Unit.MessageCaseHandlerBuilderExtensionsTests;

public class RespondContentXmlTests
{
  [Fact]
  public async Task ReturnsArrangedResponse()
  {
    var value = Guid.NewGuid().ToString();
    var responseSerializer = XmlSerialization.GetSerializer(typeof(FauxResponse));
    var caseHandler = new MessageCaseHandlerBuilder()
      .AcceptAll()
      .RespondContentXml(
        HttpStatusCode.OK,
        new FauxResponse { value = value }
      )
      .Build();
    var requestMessage = new HttpRequestMessage(HttpMethod.Patch, "https://not-real");
    var capturedRequest = await CapturedHttpRequestMessage.FromHttpRequestMessage(requestMessage);
    var expected = new FauxResponse { value = value };

    var response = await caseHandler.HandleMessage(capturedRequest, CancellationToken.None);
    var actual = (FauxResponse?)responseSerializer.Deserialize(await response.Content.ReadAsStreamAsync()) ??
                 new FauxResponse();

    Assert.Equal(expected, actual);
  }

  [SuppressMessage("ReSharper", "MemberCanBePrivate.Global",
    Justification = "Private types cannot be serialized by XmlSerializer")]
  public record FauxResponse
  {
    public string value { get; init; } = string.Empty;
  }
}

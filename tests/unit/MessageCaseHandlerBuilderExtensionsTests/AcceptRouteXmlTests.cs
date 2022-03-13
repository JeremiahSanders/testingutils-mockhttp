using System.Diagnostics.CodeAnalysis;
using System.Net;
using Xunit;

namespace Jds.TestingUtils.MockHttp.Tests.Unit.MessageCaseHandlerBuilderExtensionsTests;

public class AcceptRouteXmlTests
{
  [Fact]
  public async Task RejectsNonMatchingUris()
  {
    var arrangedUri = new Uri("https://not-real", UriKind.Absolute);
    var requestedUri = new Uri("https://difering", UriKind.Absolute);
    var requestedValue = 6;
    var responseSerializer = XmlSerialization.GetSerializer(typeof(ResponseObject));
    using var client = CreateMockApi(arrangedUri);

    await Assert.ThrowsAsync<InvalidOperationException>(async () => await client.PostAsync(requestedUri,
      CreateHttpContent.ApplicationXml(new RequestObject { Value = requestedValue }))
    );
  }

  [Fact]
  public async Task AcceptsArrangedRoute()
  {
    var arrangedUri = new Uri("https://not-real", UriKind.Absolute);
    var requestedUri = arrangedUri;
    var requestedValue = 6;
    var expected = new ResponseObject { Value = requestedValue.ToString() };
    var responseSerializer = XmlSerialization.GetSerializer(typeof(ResponseObject));
    using var client = CreateMockApi(arrangedUri);

    var response = await client.PostAsync(requestedUri,
      CreateHttpContent.ApplicationXml(new RequestObject { Value = requestedValue }));
    var deserialized = (ResponseObject?)responseSerializer.Deserialize(await response.Content.ReadAsStreamAsync()) ??
                       new ResponseObject();

    Assert.Equal(expected, deserialized);
  }

  private static HttpClient CreateMockApi(Uri? postRequestUri)
  {
    return new MockHttpBuilder()
      .WithHandler("has-a-non-default-value", caseBuilder =>
        caseBuilder.AcceptRouteXml(
            (method, uri, request) =>
              method == HttpMethod.Post && uri == postRequestUri && request.Value != default(int),
            new RequestObject()
          )
          .RespondDerivedContentXml(
            (requestObject, token) => Task.FromResult(HttpStatusCode.OK),
            (requestObject, token) => Task.FromResult(new ResponseObject { Value = requestObject.Value.ToString() }),
            new RequestObject())
      )
      .BuildHttpClient();
  }

  [SuppressMessage("ReSharper", "MemberCanBePrivate.Global",
    Justification = "Private types cannot be serialized by XmlSerializer")]
  public record RequestObject
  {
    public int Value { get; init; }
  }

  [SuppressMessage("ReSharper", "MemberCanBePrivate.Global",
    Justification = "Private types cannot be serialized by XmlSerializer")]
  public record ResponseObject
  {
    public string Value { get; init; } = string.Empty;
  }
}

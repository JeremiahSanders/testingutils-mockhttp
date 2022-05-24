using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using Xunit;

namespace Jds.TestingUtils.MockHttp.Tests.Unit.MessageCaseHandlerBuilderExtensionsTests;

public class AcceptRouteJsonTests
{
  [Fact]
  public async Task RejectsNonMatchingUris()
  {
    var arrangedUri = new Uri("https://not-real", UriKind.Absolute);
    var requestedUri = new Uri("https://difering", UriKind.Absolute);
    var requestedValue = 6;
    using var client = CreateMockApi(arrangedUri);

    await Assert.ThrowsAsync<UnhandledHttpRequestException>(async () => await client.PostAsync(requestedUri,
      CreateHttpContent.ApplicationJson(new RequestObject { Value = requestedValue }))
    );
  }

  [Fact]
  public async Task SafelyHandlesDeserializationErrors()
  {
    var arrangedUri = new Uri("https://not-real", UriKind.Absolute);
    var requestedUri = arrangedUri;
    var requestedValue = 6;
    const HttpStatusCode expected = HttpStatusCode.BadRequest;
    using var client = CreateMockApi(arrangedUri);

    var response = await client.PostAsync(requestedUri,
      new StringContent("not json", Encoding.UTF8, MediaTypeNames.Application.Json));
    var actual = response.StatusCode;

    Assert.Equal(expected, actual);
  }

  [Fact]
  public async Task AcceptsArrangedRoute()
  {
    var arrangedUri = new Uri("https://not-real", UriKind.Absolute);
    var requestedUri = arrangedUri;
    var requestedValue = 6;
    var expected = new ResponseObject { Value = requestedValue.ToString() };
    using var client = CreateMockApi(arrangedUri);

    var response = await client.PostAsync(requestedUri,
      CreateHttpContent.ApplicationJson(new RequestObject { Value = requestedValue }));
    var deserialized = await response.Content.ReadFromJsonAsync<ResponseObject>();

    Assert.Equal(expected, deserialized);
  }

  private static HttpClient CreateMockApi(Uri? postRequestUri)
  {
    return new MockHttpBuilder()
      .WithHandler("has-a-non-default-value", caseBuilder =>
        caseBuilder.AcceptRouteJson(
            (method, uri, request) =>
              method == HttpMethod.Post && uri == postRequestUri && request.Value != default(int),
            new RequestObject()
          )
          .RespondDerivedContentJson(
            (requestObject, token) => Task.FromResult(HttpStatusCode.OK),
            (requestObject, token) => Task.FromResult(new ResponseObject { Value = requestObject.Value.ToString() }),
            new RequestObject())
      )
      .WithHandler("default-value", caseBuilder =>
        caseBuilder.AcceptRouteJson(
            (method, uri, request) =>
              method == HttpMethod.Post && uri == postRequestUri && request.Value == default(int),
            new RequestObject()
          )
          .RespondStatusCode(HttpStatusCode.BadRequest)
      )
      .BuildHttpClient();
  }

  private record RequestObject
  {
    public int Value { get; init; }
  }

  private record ResponseObject
  {
    public string Value { get; init; } = string.Empty;
  }
}

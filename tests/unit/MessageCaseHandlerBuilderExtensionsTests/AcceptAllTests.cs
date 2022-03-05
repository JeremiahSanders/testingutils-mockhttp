using System.Net;
using Xunit;

namespace Jds.TestingUtils.MockHttp.Tests.Unit.ArrangedHandlerBuilderExtensionsTests;

public class AcceptAllTests
{
  public static IEnumerable<object[]> GetHttpMethods()
  {
    return TestData.HttpMethods.Select(method => new object[] { method });
  }

  [Theory]
  [MemberData(nameof(GetHttpMethods))]
  public async Task AcceptsAnyUriAndMethod(HttpMethod method)
  {
    var uri = new Uri($"https://{Guid.NewGuid():D}");
    var arrangedStatusCode = HttpStatusCode.OK;
    var client = new MockHttpBuilder()
      .WithHandler(builder => builder.AcceptAll().RespondStatusCode(arrangedStatusCode))
      .BuildHttpClient();

    var actual = (await client.SendAsync(new HttpRequestMessage(method, uri))).StatusCode;

    Assert.Equal(arrangedStatusCode, actual);
  }
}

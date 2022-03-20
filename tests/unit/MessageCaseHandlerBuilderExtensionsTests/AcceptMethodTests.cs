using System.Net;
using Xunit;

namespace Jds.TestingUtils.MockHttp.Tests.Unit.ArrangedHandlerBuilderExtensionsTests;

public class AcceptMethodTests
{
  public static IEnumerable<object[]> GetHttpMethods()
  {
    return TestData.HttpMethods.Select(method => new object[] { method });
  }

  [Theory]
  [MemberData(nameof(GetHttpMethods))]
  public async Task AcceptsAnyUriMatchingMethod(HttpMethod method)
  {
    var uri = new Uri($"https://{Guid.NewGuid():D}");
    var arrangedStatusCode = HttpStatusCode.OK;
    var client = new MockHttpBuilder()
      .WithHandler(builder => builder.AcceptMethod(method).RespondStatusCode(arrangedStatusCode))
      .BuildHttpClient();

    var actual = (await client.SendAsync(new HttpRequestMessage(method, uri))).StatusCode;

    Assert.Equal(arrangedStatusCode, actual);
  }

  [Theory]
  [MemberData(nameof(GetHttpMethods))]
  public async Task RejectsOtherMethods(HttpMethod method)
  {
    var uri = new Uri($"https://{Guid.NewGuid():D}");
    var arrangedStatusCode = HttpStatusCode.OK;
    var client = new MockHttpBuilder()
      .WithHandler(builder => builder.AcceptMethod(method).RespondStatusCode(arrangedStatusCode))
      .BuildHttpClient();
    var differingMethod = TestData.HttpMethods.First(otherMethod => otherMethod != method);

    await Assert.ThrowsAsync<InvalidOperationException>(async () =>
    {
      await client.SendAsync(new HttpRequestMessage(differingMethod, uri));
    });
  }
}

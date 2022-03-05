using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Jds.TestingUtils.MockHttp.Tests.Unit;

public class CompleteApiTests
{
  public static IEnumerable<object[]> GetHttpMethods()
  {
    return TestData.HttpMethods.Select(method => new object[] { method });
  }

  [Theory]
  [MemberData(nameof(GetHttpMethods))]
  public async Task ArrangedUriAllMethods_ReturnsExpectedStatusCode(HttpMethod httpMethod)
  {
    using var testClient = MockApi.CreateCompleteApi();
    var expected = HttpStatusCode.OK;

    var actual = (await testClient.SendAsync(new HttpRequestMessage(httpMethod, MockApi.BaseUri)))
      .StatusCode;

    Assert.Equal(expected, actual);
  }

  [Fact]
  public async Task ArrangedStatusCodeHead_ReturnsExpectedStatusCode()
  {
    using var testClient = MockApi.CreateCompleteApi();
    var expected = HttpStatusCode.OK;

    var actual = (await testClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, MockApi.PlainTextGetRoute)))
      .StatusCode;

    Assert.Equal(expected, actual);
  }

  [Fact]
  public async Task ArrangedPlainTextGet_ReturnsExpectedBody()
  {
    using var testClient = MockApi.CreateCompleteApi();
    var expected = MockApi.PlainTextGetBody;

    var actual = await testClient.GetStringAsync(MockApi.PlainTextGetRoute);

    Assert.Equal(expected, actual);
  }

  [Fact]
  public async Task ArrangedJsonPost_ReturnsExpectedBody()
  {
    using var testClient = MockApi.CreateCompleteApi();
    var request = new MockApi.SumIntsJsonRequest { Ints = new[] { 1, 2, 3 } };
    var expected = new MockApi.SumIntsJsonResponse { Sum = request.Ints.Sum() };

    var response = await testClient.PostAsJsonAsync(MockApi.SumIntsJsonPostRoute, request);
    var actual = await response.Content.ReadFromJsonAsync<MockApi.SumIntsJsonResponse>();

    Assert.Equal(expected, actual);
  }

  [Fact]
  public async Task ArrangedJsonPost_UsesDefaultRequestIfDeserializationFails()
  {
    using var testClient = MockApi.CreateCompleteApi();
    var expected = new MockApi.SumIntsJsonResponse { Sum = 0 };

    var response =
      await testClient.PostAsync(MockApi.SumIntsJsonPostRoute, new ByteArrayContent(new byte[] { 1, 2, 3 }));
    var actual = await response.Content.ReadFromJsonAsync<MockApi.SumIntsJsonResponse>();

    Assert.Equal(expected, actual);
  }
}

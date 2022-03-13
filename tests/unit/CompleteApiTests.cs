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

    var actual =
      (await testClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, MockApi.PlainTextGetRoute)))
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
      await testClient.PostAsync(MockApi.SumIntsJsonPostRoute,
        new ByteArrayContent(new byte[] { 1, 2, 3 }));
    var actual = await response.Content.ReadFromJsonAsync<MockApi.SumIntsJsonResponse>();

    Assert.Equal(expected, actual);
  }

  [Theory]
  [InlineData(3, 6)]
  [InlineData(1, 8)]
  [InlineData(-500, 3720)]
  public async Task StatefulArrangements_ReturnExpectedResults(int firstValue, int secondValue)
  {
    using var testClient = MockApi.CreateCompleteApi();

    var add1Response = await testClient.PostAsJsonAsync(MockApi.StatefulAddPostRoute,
      new MockApi.StatefulRequest { Value = firstValue });
    var add2Response = await testClient.PostAsJsonAsync(MockApi.StatefulAddPostRoute,
      new MockApi.StatefulRequest { Value = secondValue });
    var get1Response = await testClient.GetFromJsonAsync<int[]>(MockApi.StatefulGetRoute);
    var remove1Response = await testClient.PostAsJsonAsync(MockApi.StatefulRemovePostRoute,
      new MockApi.StatefulRequest { Value = firstValue });
    var get2Response = await testClient.GetFromJsonAsync<int[]>(MockApi.StatefulGetRoute);
    var remove2Response = await testClient.PostAsJsonAsync(MockApi.StatefulRemovePostRoute,
      new MockApi.StatefulRequest { Value = secondValue });
    var get3Response = await testClient.GetFromJsonAsync<int[]>(MockApi.StatefulGetRoute);

    Assert.Equal(HttpStatusCode.OK, add1Response.StatusCode);
    Assert.Equal(HttpStatusCode.OK, add2Response.StatusCode);
    Assert.Equal(HttpStatusCode.OK, remove1Response.StatusCode);
    Assert.Equal(HttpStatusCode.OK, remove2Response.StatusCode);

    var expectedGet1 = new[] { firstValue, secondValue }.OrderBy(value => value);
    var expectedGet2 = new[] { secondValue }.OrderBy(value => value);
    var expectedGet3 = Array.Empty<int>();
    Assert.Equal(expectedGet1, get1Response!.OrderBy(value => value));
    Assert.Equal(expectedGet2, get2Response!.OrderBy(value => value));
    Assert.Equal(expectedGet3, get3Response);
  }

  [Fact]
  public async Task StatefulAdd_GivenNullValue_ReturnsBadRequest()
  {
    using var testClient = MockApi.CreateCompleteApi();
    const HttpStatusCode expected = HttpStatusCode.BadRequest;

    var addNull =
      await testClient.PostAsJsonAsync(MockApi.StatefulAddPostRoute,
        new MockApi.StatefulRequest { Value = null });

    Assert.Equal(expected, addNull.StatusCode);
  }
}

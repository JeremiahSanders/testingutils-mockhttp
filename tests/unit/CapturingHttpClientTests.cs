using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace Jds.TestingUtils.MockHttp.Tests.Unit;

public class CapturingHttpClientTests : IAsyncLifetime
{
  private readonly ITestOutputHelper _testOutputHelper;
  private readonly CapturingHttpClient capturingHttpClient;
  private HttpResponseMessage firstResponse;
  private string firstResponseStringBody;
  private HttpResponseMessage secondResponse;
  private string secondResponseStringBody;

  public CapturingHttpClientTests(ITestOutputHelper testOutputHelper)
  {
    _testOutputHelper = testOutputHelper;
    capturingHttpClient = new CapturingHttpClient();
  }

  public async Task InitializeAsync()
  {
    const string baseApiRoute = "https://randomuser.me/api/";
    const string alphaSeed = baseApiRoute + "?seed=" + "alpha";
    const string betaSeed = baseApiRoute + "?seed=" + "beta";

    firstResponse = await capturingHttpClient.GetAsync(alphaSeed);
    secondResponse = await capturingHttpClient.GetAsync(betaSeed);

    firstResponseStringBody = await firstResponse.Content.ReadAsStringAsync();
    secondResponseStringBody = await secondResponse.Content.ReadAsStringAsync();

    var jsonSerializerOptions = new JsonSerializerOptions
    {
      WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    _testOutputHelper.WriteLine(string.Join("\n\n",
        capturingHttpClient.CapturedRequestResponses
          .Select(kvp => JsonSerializer.Serialize(kvp, jsonSerializerOptions)
          )
      )
    );
  }

  public Task DisposeAsync()
  {
    capturingHttpClient.Dispose();
    return Task.CompletedTask;
  }

  [Fact]
  public void CapturesInteractions()
  {
    Assert.Equal(2, capturingHttpClient.CapturedRequestResponses.Count);
  }

  [Fact]
  public void CapturedAlphaContentMatchesSource()
  {
    var expected = firstResponseStringBody;
    var actual = capturingHttpClient.CapturedRequestResponses[0].Response.ContentString;

    Assert.Equal(expected, actual);
  }

  [Fact]
  public void CapturedBetaContentMatchesSource()
  {
    var expected = secondResponseStringBody;
    var actual = capturingHttpClient.CapturedRequestResponses[1].Response.ContentString;

    Assert.Equal(expected, actual);
  }
}

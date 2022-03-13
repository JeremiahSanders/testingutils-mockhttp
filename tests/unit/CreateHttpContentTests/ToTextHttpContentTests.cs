using Xunit;

namespace Jds.TestingUtils.MockHttp.Tests.Unit.CreateHttpContentTests;

public class ToTextHttpContentTests
{
  [Fact]
  public async Task ConvertsStringToReadableHttpContent()
  {
    const string expected = "this is a sample string";

    var converted = expected.ToTextHttpContent();
    var actual = await converted.ReadAsStringAsync();

    Assert.Equal(expected, actual);
  }
}

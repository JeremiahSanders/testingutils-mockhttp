using Xunit;

namespace Jds.TestingUtils.MockHttp.Tests.Unit.UriExtensionsTests;

public class QueryValueContainsTests
{
  private const string ParameterKey = "startsWith";
  private const string ParameterValue1 = "one";
  private const string ParameterValue2 = "two";
  private const string ParameterValue3 = "three";
  private readonly Uri _noQueryString = new("https://a-fake-url");
  private readonly Uri _noQueryStringUriContainsKey = new("https://a-fake-url/startsWith");

  private readonly Uri _presentArray =
    new(
      $"https://a-fake-url/something?{ParameterKey}={ParameterValue1}&{ParameterKey}={ParameterValue2}&{ParameterKey}={ParameterValue3}");

  private readonly Uri _presentNoValue = new($"https://a-fake-url/something?{ParameterKey}=");

  private readonly Uri _presentValue1 =
    new($"https://a-fake-url/something?{ParameterKey}={ParameterValue1}");

  private readonly Uri _presentWithoutEqualsOrValue = new($"https://a-fake-url/something?{ParameterKey}");

  [Fact]
  public void GivenNoQueryString_ReturnsFalse()
  {
    var actual = _noQueryString.QueryValueContains(ParameterKey, string.Empty);

    Assert.False(actual);
  }

  [Fact]
  public void GivenNoQueryStringButKeyIsInRoute_ReturnsFalse()
  {
    var actual = _noQueryStringUriContainsKey.QueryValueContains(ParameterKey, string.Empty);

    Assert.False(actual);
  }

  [Fact]
  public void Given_ParameterExists_NoValue_ExpectEmpty_ReturnsTrue()
  {
    var actual = _presentNoValue.QueryValueContains(ParameterKey, string.Empty);

    Assert.True(actual);
  }

  [Fact]
  public void Given_ParameterExists_WithoutEqualsOrValue_ExpectEmpty_ReturnsFalse()
  {
    var actual = _presentWithoutEqualsOrValue.QueryValueContains(ParameterKey, string.Empty);

    Assert.False(actual);
  }

  [Fact]
  public void Given_ParameterExists_SpecifiedValue_ExpectKnownValue_ReturnsTrue()
  {
    var actual = _presentValue1.QueryValueContains(ParameterKey, ParameterValue1);

    Assert.True(actual);
  }

  [Theory]
  [InlineData(ParameterValue1)]
  [InlineData(ParameterValue2)]
  [InlineData(ParameterValue3)]
  public void Given_ParameterExists_ValueArray_ExpectKnownValue_ReturnsTrue(string valueToCheck)
  {
    var actual = _presentArray.QueryValueContains(ParameterKey, valueToCheck);

    Assert.True(actual);
  }
}

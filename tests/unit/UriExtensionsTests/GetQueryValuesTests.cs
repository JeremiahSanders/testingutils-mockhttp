using Xunit;

namespace Jds.TestingUtils.MockHttp.Tests.Unit.UriExtensionsTests;

public class GetQueryValuesTests
{
  private readonly Uri _baseUri;

  public GetQueryValuesTests()
  {
    _baseUri = new Uri("https://base-site", UriKind.Absolute);
  }


  [Fact]
  public void GivenNoQuery_ReturnsEmptyArray()
  {
    var expected = Array.Empty<string>();

    var actual = _baseUri.GetQueryValues(Guid.NewGuid().ToString());

    Assert.Equal(expected, actual);
  }

  [Fact]
  public void GivenNoMatchingQueryParam_ReturnsEmptyArray()
  {
    var singleParamKey = Guid.NewGuid().ToString();
    var singleParamValue = Guid.NewGuid().ToString();
    var singleParam = new Uri(_baseUri, $"section/item/collection?{singleParamKey}={singleParamValue}");
    var expected = Array.Empty<string>();

    var actual = singleParam.GetQueryValues(Guid.NewGuid().ToString());

    Assert.Equal(expected, actual);
  }

  [Fact]
  public void GivenSingleMatchingQueryParam_ReturnsValue()
  {
    var singleParamKey = Guid.NewGuid().ToString();
    var singleParamValue = Guid.NewGuid().ToString();
    var singleParam = new Uri(_baseUri, $"section/item/collection?{singleParamKey}={singleParamValue}");
    var expected = new[] { singleParamValue };

    var actual = singleParam.GetQueryValues(singleParamKey);

    Assert.Equal(expected, actual);
  }

  [Fact]
  public void GivenDuplicatedMatchingQueryParam_ReturnsValues()
  {
    var arrayDuplicatedKeyKey = Guid.NewGuid().ToString();
    var arrayDuplicatedKeyValue1 = Guid.NewGuid().ToString();
    var arrayDuplicatedKeyValue2 = Guid.NewGuid().ToString();
    var arrayDuplicatedKey = new Uri(_baseUri,
      $"search?{arrayDuplicatedKeyKey}={arrayDuplicatedKeyValue1}&{arrayDuplicatedKeyKey}={arrayDuplicatedKeyValue2}");
    var expected = new[] { arrayDuplicatedKeyValue1, arrayDuplicatedKeyValue2 };

    var actual = arrayDuplicatedKey.GetQueryValues(arrayDuplicatedKeyKey);

    Assert.Equal(expected, actual);
  }
}

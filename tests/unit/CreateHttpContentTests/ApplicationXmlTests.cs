using Xunit;

namespace Jds.TestingUtils.MockHttp.Tests.Unit.CreateHttpContentTests;

public class ApplicationXmlTests
{
  [Fact]
  public void GivenNullValue_ThrowsArgumentNullException()
  {
    Assert.Throws<ArgumentNullException>(
      () => CreateHttpContent.ApplicationXml<string>(null!)
    );
  }
}

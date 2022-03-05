using Xunit;

namespace Jds.TestingUtils.MockHttp.Tests.Unit;

public class ArrangedHttpMessageHandlerTests
{
  [Fact]
  public void CanClearHandlers()
  {
    using var handler = new ArrangedHttpMessageHandler();
    handler.Add(new MessageCaseHandlerBuilder().Build());

    handler.Clear();

    Assert.Empty(handler);
  }

  [Fact]
  public void CanRemoveHandlers()
  {
    var id = Guid.NewGuid().ToString("N");
    using var handler = new ArrangedHttpMessageHandler();
    handler.Add(id, new MessageCaseHandlerBuilder().Build());

    handler.Remove(id);

    Assert.Throws<KeyNotFoundException>(() => handler[id]);
  }

  [Fact]
  public void CanAccessHandlersById()
  {
    var id = Guid.NewGuid().ToString("N");
    using var handler = new ArrangedHttpMessageHandler();
    var caseHandler = new MessageCaseHandlerBuilder().Build();

    handler.Add(id, caseHandler);
    var indexedCaseHandler = handler[id];

    Assert.Equal(caseHandler, indexedCaseHandler);
  }

  [Fact]
  public void ReturnsExpectedCount()
  {
    var id = Guid.NewGuid().ToString("N");
    using var handler = new ArrangedHttpMessageHandler();
    var caseHandler = new MessageCaseHandlerBuilder().Build();
    const int expected = 1;

    handler.Add(id, caseHandler);
    var count = handler.Count;

    Assert.Equal(expected, count);
  }

  [Fact]
  public void CanEnumerateCaseHandlers()
  {
    var id = Guid.NewGuid().ToString("N");
    using var handler = new ArrangedHttpMessageHandler();
    var caseHandler = new MessageCaseHandlerBuilder().Build();
    handler.Add(id, caseHandler);

    foreach (var keyValuePair in handler)
    {
      Assert.Equal(id, keyValuePair.Key);
      Assert.Equal(caseHandler, keyValuePair.Value);
    }
  }

  private static HttpClient ToHttpClient(ArrangedHttpMessageHandler handler)
  {
    return new HttpClient(handler, false);
  }
}

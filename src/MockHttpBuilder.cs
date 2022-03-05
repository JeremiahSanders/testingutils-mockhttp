namespace Jds.TestingUtils.MockHttp;

/// <summary>
///   A builder which creates mocked, arranged <see cref="HttpClient" /> and <see cref="HttpMessageHandler" /> objects.
/// </summary>
public class MockHttpBuilder
{
  private readonly List<string> _handlerOrder;

  /// <summary>
  ///   Initializes a new instance of the <see cref="MockHttpBuilder" /> class.
  /// </summary>
  public MockHttpBuilder()
  {
    MessageHandlers = new Dictionary<string, MessageCaseHandler>();
    _handlerOrder = new List<string>();
  }

  private Dictionary<string, MessageCaseHandler> MessageHandlers { get; }

  /// <summary>
  ///   Add a <see cref="MessageCaseHandler" /> identified by <paramref name="id" />. If a handler is already registered for
  ///   <paramref name="id" />, it will be replaced.
  /// </summary>
  /// <param name="id">A message case identifier.</param>
  /// <param name="handler">A <see cref="MessageCaseHandler" />.</param>
  /// <returns>This instance.</returns>
  public MockHttpBuilder WithMessageCaseHandler(string id, MessageCaseHandler handler)
  {
    if (!MessageHandlers.ContainsKey(id))
    {
      _handlerOrder.Add(id);
    }

    MessageHandlers[id] = handler;

    return this;
  }

  /// <summary>
  ///   Add a <see cref="MessageCaseHandler" />.
  /// </summary>
  /// <param name="handler">A <see cref="MessageCaseHandler" />.</param>
  /// <returns>This instance.</returns>
  public MockHttpBuilder WithMessageCaseHandler(MessageCaseHandler handler)
  {
    return WithMessageCaseHandler(Guid.NewGuid().ToString(), handler);
  }

  /// <summary>
  ///   Add a <see cref="MessageCaseHandler" /> by providing a <see cref="MessageCaseHandlerBuilder" />
  ///   <paramref name="fluentArrangement" />.
  /// </summary>
  /// <param name="id">A message case identifier.</param>
  /// <param name="fluentArrangement">A <see cref="Func{T,TResult}" /> which builds a <see cref="MessageCaseHandler" />.</param>
  /// <returns>This instance.</returns>
  public MockHttpBuilder WithHandler(
    string id,
    Func<MessageCaseHandlerBuilder, MessageCaseHandlerBuilder> fluentArrangement
  )
  {
    return WithMessageCaseHandler(id, fluentArrangement(new MessageCaseHandlerBuilder()).Build());
  }

  /// <summary>
  ///   Add a <see cref="MessageCaseHandler" /> by providing a <see cref="MessageCaseHandlerBuilder" />
  ///   <paramref name="fluentArrangement" />.
  /// </summary>
  /// <param name="fluentArrangement">A <see cref="Func{T,TResult}" /> which builds a <see cref="MessageCaseHandler" />.</param>
  /// <returns>This instance.</returns>
  public MockHttpBuilder WithHandler(
    Func<MessageCaseHandlerBuilder, MessageCaseHandlerBuilder> fluentArrangement
  )
  {
    return WithHandler(Guid.NewGuid().ToString(), fluentArrangement);
  }

  /// <summary>
  ///   Builds an <see cref="HttpClient" /> which responds according to the current arrangement.
  /// </summary>
  /// <returns>An <see cref="HttpClient" />.</returns>
  public HttpClient BuildHttpClient()
  {
    return new HttpClient(BuildArrangedHttpMessageHandler(), true);
  }

  /// <summary>
  ///   Builds an <see cref="HttpMessageHandler" /> which handles messages according to the current arrangement.
  /// </summary>
  /// <returns>An <see cref="HttpMessageHandler" />.</returns>
  public HttpMessageHandler BuildHttpMessageHandler()
  {
    return BuildArrangedHttpMessageHandler();
  }

  private ArrangedHttpMessageHandler BuildArrangedHttpMessageHandler()
  {
    var handler = new ArrangedHttpMessageHandler();

    foreach (var handlerKey in _handlerOrder)
    {
      handler.Add(handlerKey, MessageHandlers[handlerKey]);
    }

    return handler;
  }
}

using System.Collections;

namespace Jds.TestingUtils.MockHttp;

/// <summary>
///   A <see cref="HttpMessageHandler" /> which uses prearranged <see cref="MessageCaseHandler" /> to handle messages.
/// </summary>
public class ArrangedHttpMessageHandler : HttpMessageHandler,
  IReadOnlyCollection<KeyValuePair<string, MessageCaseHandler>>
{
  private readonly List<string> _handlerOrder;

  /// <summary>
  ///   Initializes a new instance of class <see cref="ArrangedHttpMessageHandler" />.
  /// </summary>
  public ArrangedHttpMessageHandler()
  {
    ArrangedHandlers = new Dictionary<string, MessageCaseHandler>();
    _handlerOrder = new List<string>();
  }

  private Dictionary<string, MessageCaseHandler> ArrangedHandlers { get; }

  /// <summary>
  ///   Gets the <see cref="MessageCaseHandler" /> registered with <paramref name="identifier" />.
  /// </summary>
  /// <param name="identifier">A handler identifier.</param>
  public MessageCaseHandler this[string identifier] => ArrangedHandlers[identifier];

  /// <inheritdoc cref="IEnumerable.GetEnumerator" />
  public IEnumerator<KeyValuePair<string, MessageCaseHandler>> GetEnumerator()
  {
    return ArrangedHandlers.GetEnumerator();
  }

  /// <inheritdoc cref="IEnumerable.GetEnumerator" />
  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }

  /// <inheritdoc cref="IReadOnlyCollection{T}.Count" />
  public int Count => ArrangedHandlers.Count;

  /// <summary>
  ///   Add <paramref name="handler" /> to arranged handlers with a specified identifier.
  /// </summary>
  /// <param name="identifier">A handler identifier.</param>
  /// <param name="handler">A <see cref="MessageCaseHandler" />.</param>
  public void Add(string identifier, MessageCaseHandler handler)
  {
    if (!ArrangedHandlers.ContainsKey(identifier))
    {
      _handlerOrder.Add(identifier);
    }

    ArrangedHandlers[identifier] = handler;
  }

  /// <summary>
  ///   Add <paramref name="handler" /> to arranged handlers with a random identifier.
  /// </summary>
  /// <param name="handler">A <see cref="MessageCaseHandler" />.</param>
  public void Add(MessageCaseHandler handler)
  {
    Add(
      Guid.NewGuid().ToString(),
      handler
    );
  }

  /// <summary>
  ///   Removes a handler, identified by <paramref name="identifier" />.
  /// </summary>
  /// <param name="identifier">A handler identifier.</param>
  /// <returns>
  ///   true if the element is successfully found and removed; otherwise, false. This method returns false if no
  ///   handler was previously registered for the <paramref name="identifier" />.
  /// </returns>
  public bool Remove(string identifier)
  {
    return _handlerOrder.Remove(identifier) && ArrangedHandlers.Remove(identifier);
  }

  /// <summary>
  ///   Removes all handlers.
  /// </summary>
  public void Clear()
  {
    _handlerOrder.Clear();
    ArrangedHandlers.Clear();
  }

  /// <inheritdoc cref="HttpMessageHandler.SendAsync" />
  /// <exception cref="InvalidOperationException">
  ///   Thrown when no registered <see cref="MessageCaseHandler" /> handles the
  ///   request.
  /// </exception>
  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
    CancellationToken cancellationToken)
  {
    var handler = await TryGetHandler(request) ??
                  throw new InvalidOperationException(
                    $"No handlers registered for request. {request.Method} {request.RequestUri}");

    return await handler(request, cancellationToken);
  }

  private async Task<Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>?> TryGetHandler(
    HttpRequestMessage request)
  {
    foreach (var handlerIdentifier in _handlerOrder)
    {
      var arrangedHandler = ArrangedHandlers[handlerIdentifier];
      if (await arrangedHandler.DoesHandleMessage(request))
      {
        return arrangedHandler.HandleMessage;
      }
    }

    return null;
  }
}

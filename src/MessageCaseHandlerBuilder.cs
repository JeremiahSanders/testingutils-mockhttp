using System.Diagnostics.CodeAnalysis;

namespace Jds.TestingUtils.MockHttp;

/// <summary>
///   A builder for <see cref="MessageCaseHandler" />.
/// </summary>
public class MessageCaseHandlerBuilder
{
  private readonly List<Func<HttpRequestMessage, Task<bool>>> _acceptRules;
  private Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _responseHandler;

  /// <summary>
  ///   Initializes a new instance of the <see cref="MessageCaseHandlerBuilder" /> class.
  /// </summary>
  /// <exception cref="InvalidOperationException"></exception>
  public MessageCaseHandlerBuilder()
  {
    _acceptRules = new List<Func<HttpRequestMessage, Task<bool>>>();
    _responseHandler =
      [ExcludeFromCodeCoverage](_, _) => throw new InvalidOperationException("Response not configured.");
  }


  /// <summary>
  ///   Adds a new <paramref name="acceptRule" /> to the <see cref="MessageCaseHandler" />.
  /// </summary>
  /// <remarks>
  ///   Rules are evaluated sequentially, in order of addition, during
  ///   <see cref="MessageCaseHandler.DoesHandleMessage" />.
  /// </remarks>
  /// <param name="acceptRule">
  ///   A <see cref="Func{T,TResult}" /> which asynchronously determines if the
  ///   <see cref="MessageCaseHandler" /> will handle the message.
  /// </param>
  /// <returns>This instance.</returns>
  public MessageCaseHandlerBuilder WithAcceptRule(Func<HttpRequestMessage, Task<bool>> acceptRule)
  {
    _acceptRules.Add(acceptRule);

    return this;
  }

  /// <summary>
  ///   Sets the asynchronous response handler for the <see cref="MessageCaseHandler" />.
  /// </summary>
  /// <param name="responseHandler">A <see cref="Func{T1,T2,TResult}" /> response handler.</param>
  /// <returns>This instance</returns>
  public MessageCaseHandlerBuilder SetResponseHandler(
    Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseHandler
  )
  {
    _responseHandler = responseHandler;

    return this;
  }

  /// <summary>
  ///   Builds a <see cref="MessageCaseHandler" /> from the accept rules and response handler currently configured.
  /// </summary>
  /// <returns>A <see cref="MessageCaseHandler" />.</returns>
  public MessageCaseHandler Build()
  {
    return new MessageCaseHandler(CreateDoesHandleMessage(), _responseHandler);
  }

  private Func<HttpRequestMessage, Task<bool>> CreateDoesHandleMessage()
  {
    // Shallow-clone the accept rules, to prevent later builder modifications changing the accept rules reference value.
    var currentAcceptRules = new List<Func<HttpRequestMessage, Task<bool>>>(_acceptRules);

    async Task<bool> DoesHandle(HttpRequestMessage message)
    {
      foreach (var acceptRule in currentAcceptRules)
      {
        if (await acceptRule(message))
        {
          return true;
        }
      }

      return false;
    }

    return DoesHandle;
  }
}

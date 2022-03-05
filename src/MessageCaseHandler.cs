namespace Jds.TestingUtils.MockHttp;

/// <summary>
///   An <see cref="HttpRequestMessage" /> handler, arranged to handle a specific test case.
/// </summary>
/// <param name="DoesHandleMessage">A <see cref="Func{T,TResult}" /> which determines if a message is handled.</param>
/// <param name="HandleMessage">A <see cref="Func{T,TResult}" /> which provides a <see cref="HttpResponseMessage" />.</param>
public record MessageCaseHandler(
  Func<HttpRequestMessage, Task<bool>> DoesHandleMessage,
  Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> HandleMessage
);

namespace Jds.TestingUtils.MockHttp;

/// <summary>
///   An <see cref="HttpMessageHandler" /> which captures all requests and responses.
/// </summary>
/// <remarks>
///   <para>
///     This class aids the creation of mocked HTTP handlers, by capturing the messages sent and received from real
///     interactions.
///   </para>
/// </remarks>
public class CapturingHttpMessageHandler : HttpMessageHandler
{
  private readonly List<CapturedHttpRequestResponse> _capturedRequestResponses;
  private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _ioHandler;

  /// <summary>
  ///   Initializes a new instance of the <see cref="CapturingHttpMessageHandler" /> class.
  /// </summary>
  /// <param name="ioHandler">A <see cref="Func{T1,T2,TResult}" /> which handles message requests.</param>
  public CapturingHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> ioHandler)
  {
    _ioHandler = ioHandler;
    _capturedRequestResponses = new List<CapturedHttpRequestResponse>();
  }

  /// <summary>
  ///   Gets the captured requests.
  /// </summary>
  public IReadOnlyList<CapturedHttpRequestResponse> CapturedRequestResponses => _capturedRequestResponses;

  /// <inheritdoc cref="HttpMessageHandler.SendAsync" />
  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
    CancellationToken cancellationToken)
  {
    var (clonedMessage, capturedRequest) = await HttpMessageCloner.CloneRequest(request, cancellationToken);
    var response = await _ioHandler(clonedMessage, cancellationToken);
    var (clonedResponse, capturedResponse) = await HttpMessageCloner.CloneResponse(response, cancellationToken);
    _capturedRequestResponses.Add(new CapturedHttpRequestResponse(capturedRequest, capturedResponse));

    return clonedResponse;
  }
}

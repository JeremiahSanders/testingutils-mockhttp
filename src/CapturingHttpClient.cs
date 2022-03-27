namespace Jds.TestingUtils.MockHttp;

public class CapturingHttpClient : HttpClient
{
  private readonly bool _disposeHttpClient;
  private readonly CapturingHttpMessageHandler _handler;
  private readonly HttpClient? _httpClient;

  public CapturingHttpClient() : this(new HttpClient(), true)
  {
  }

  public CapturingHttpClient(HttpClient httpClient, bool disposeHttpClient) : this(httpClient.SendAsync)
  {
    _httpClient = httpClient;
    _disposeHttpClient = disposeHttpClient;
  }

  public CapturingHttpClient(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> func)
    : this(new CapturingHttpMessageHandler(func))
  {
  }

  public CapturingHttpClient(CapturingHttpMessageHandler handler, bool disposeHandler = true)
    : base(handler, disposeHandler)
  {
    _handler = handler;
  }

  /// <inheritdoc cref="CapturingHttpMessageHandler.CapturedRequestResponses"/>
  public IReadOnlyList<CapturedHttpRequestResponse> CapturedRequestResponses => _handler.CapturedRequestResponses;

  /// <inheritdoc cref="HttpClient.Dispose(bool)" />
  protected override void Dispose(bool disposing)
  {
    if (disposing && _disposeHttpClient && _httpClient != null)
    {
      _httpClient.Dispose();
    }
    
    base.Dispose(disposing);
  }
}

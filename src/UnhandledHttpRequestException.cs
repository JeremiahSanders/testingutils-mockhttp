using System.Net.Http.Headers;

namespace Jds.TestingUtils.MockHttp;

/// <summary>
///   An exception thrown when an <see cref="ArrangedHttpMessageHandler" /> receives a <see cref="HttpRequestMessage" />
///   which has no arranged handler.
/// </summary>
public class UnhandledHttpRequestException : Exception
{
  /// <summary>
  ///   Initializes a new instance of the <see cref="UnhandledHttpRequestException" /> class.
  /// </summary>
  /// <param name="capturedHttpRequestMessage">A <see cref="CapturedHttpRequestMessage" />.</param>
  public UnhandledHttpRequestException(CapturedHttpRequestMessage capturedHttpRequestMessage)
    : this(
      capturedHttpRequestMessage.Method,
      capturedHttpRequestMessage.RequestUri,
      capturedHttpRequestMessage.Headers,
      capturedHttpRequestMessage.ContentBytes,
      capturedHttpRequestMessage.ContentString
    )
  {
  }

  /// <summary>
  ///   Initializes a new instance of the <see cref="UnhandledHttpRequestException" /> class.
  /// </summary>
  /// <param name="method">A <see cref="HttpMethod" />.</param>
  /// <param name="requestUri">A <see cref="Uri" />.</param>
  /// <param name="headers">Some <see cref="HttpRequestHeaders" />.</param>
  /// <param name="contentBytes">A content body, read as a <see cref="byte" /> <see cref="Array" />.</param>
  /// <param name="contentString">A content body, read as a <see cref="string" />.</param>
  public UnhandledHttpRequestException(
    HttpMethod method,
    Uri? requestUri,
    HttpRequestHeaders headers,
    byte[]? contentBytes,
    string? contentString
  ) : base(GenerateMessage(method, requestUri))
  {
    ContentString = contentString;
    ContentBytes = contentBytes;
    Headers = headers;
    RequestUri = requestUri;
    Method = method;
  }

  /// <summary>
  ///   Gets the unhandled <see cref="HttpRequestMessage" /> <see cref="HttpRequestMessage.Content" />, read as a
  ///   <see cref="string" />.
  /// </summary>
  public string? ContentString { get; }


  /// <summary>
  ///   Gets the unhandled <see cref="HttpRequestMessage" /> <see cref="HttpRequestMessage.Content" />, read as a
  ///   <see cref="byte" /> <see cref="Array" />.
  /// </summary>
  public byte[]? ContentBytes { get; }

  /// <summary>
  ///   Gets the unhandled <see cref="HttpRequestMessage" /> <see cref="HttpRequestMessage.Headers" />.
  /// </summary>
  public HttpRequestHeaders Headers { get; }

  /// <summary>
  ///   Gets the unhandled <see cref="HttpRequestMessage" /> <see cref="HttpRequestMessage.RequestUri" />.
  /// </summary>
  public Uri? RequestUri { get; }

  /// <summary>
  ///   Gets the unhandled <see cref="HttpRequestMessage" /> <see cref="HttpRequestMessage.Method" />.
  /// </summary>
  public HttpMethod Method { get; }

  private static string GenerateMessage(HttpMethod method, Uri? requestUri)
  {
    return $"No handlers registered for request. {method} {requestUri}";
  }
}

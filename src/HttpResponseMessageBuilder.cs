using System.Net;

namespace Jds.TestingUtils.MockHttp;

/// <summary>
///   A builder for <see cref="HttpResponseMessage" />.
/// </summary>
public class HttpResponseMessageBuilder
{
  /// <summary>
  ///   <see cref="HttpStatusCode" /> which will be returned by default.
  /// </summary>
  public const HttpStatusCode DefaultHttpStatusCode = HttpStatusCode.NotImplemented;

  private readonly Dictionary<string, List<string>> _headers = new();
  private readonly Dictionary<string, List<string>> _trailingHeaders = new();

  private HttpContent? _content;
  private string? _reasonPhrase;
  private HttpRequestMessage? _requestMessage;
  private HttpStatusCode _statusCode = DefaultHttpStatusCode;
  private Version? _version;

  /// <summary>
  ///   Sets the <see cref="HttpResponseMessage.Content" />.
  /// </summary>
  /// <param name="content">An <see cref="HttpContent" />.</param>
  /// <returns>This instance.</returns>
  public HttpResponseMessageBuilder WithContent(HttpContent content)
  {
    _content = content;

    return this;
  }

  /// <summary>
  ///   Adds a header value to <see cref="HttpResponseMessage.Headers" />.
  /// </summary>
  /// <remarks>
  ///   Execute this method with the same <paramref name="name" /> and differing <paramref name="value" /> to include
  ///   multiple values for the same header (e.g., accept, allow).
  /// </remarks>
  /// <param name="name">A header name.</param>
  /// <param name="value">A header value.</param>
  /// <returns>This instance.</returns>
  public HttpResponseMessageBuilder WithHeader(string name, string value)
  {
    if (!_headers.ContainsKey(name))
    {
      _headers[name] = new List<string>();
    }

    if (!_headers[name].Contains(value))
    {
      _headers[name].Add(value);
    }

    return this;
  }


  /// <summary>
  ///   Sets the <see cref="HttpResponseMessage.StatusCode" />.
  /// </summary>
  /// <param name="statusCode">A <see cref="HttpStatusCode" />.</param>
  /// <returns>This instance.</returns>
  public HttpResponseMessageBuilder WithStatusCode(HttpStatusCode statusCode)
  {
    _statusCode = statusCode;

    return this;
  }


  /// <summary>
  ///   Adds a header value to <see cref="HttpResponseMessage.TrailingHeaders" />.
  /// </summary>
  /// <remarks>
  ///   Execute this method with the same <paramref name="name" /> and differing <paramref name="value" /> to include
  ///   multiple values for the same header (e.g., accept, allow).
  /// </remarks>
  /// <param name="name">A header name.</param>
  /// <param name="value">A header value.</param>
  /// <returns>This instance.</returns>
  public HttpResponseMessageBuilder WithTrailingHeader(string name, string value)
  {
    if (!_trailingHeaders.ContainsKey(name))
    {
      _trailingHeaders[name] = new List<string>();
    }

    if (!_trailingHeaders[name].Contains(value))
    {
      _trailingHeaders[name].Add(value);
    }

    return this;
  }

  /// <summary>
  ///   Sets the <see cref="HttpResponseMessage.Version" />.
  /// </summary>
  /// <remarks>
  ///   This method does not set an application/API version or headers. It sets the
  ///   <see cref="HttpResponseMessage.Version" />.
  ///   Overriding the default value, <c>1.1</c>, is uncommon.
  /// </remarks>
  /// <param name="version">
  ///   An HTTP message version. Per <see cref="HttpResponseMessage.Version" /> documentation, the
  ///   default is <c>1.1</c>.
  /// </param>
  /// <returns>This instance.</returns>
  public HttpResponseMessageBuilder WithVersion(Version version)
  {
    _version = version;

    return this;
  }

  /// <summary>
  ///   Sets the <see cref="HttpResponseMessage.ReasonPhrase" />.
  /// </summary>
  /// <param name="reasonPhrase">A reason phrase.</param>
  /// <returns>This instance.</returns>
  public HttpResponseMessageBuilder WithReasonPhrase(string reasonPhrase)
  {
    _reasonPhrase = reasonPhrase;

    return this;
  }

  /// <summary>
  ///   Sets the <see cref="HttpResponseMessage.RequestMessage" />.
  /// </summary>
  /// <param name="requestMessage">A <see cref="HttpRequestMessage" />.</param>
  /// <returns>This instance.</returns>
  public HttpResponseMessageBuilder WithRequestMessage(HttpRequestMessage requestMessage)
  {
    _requestMessage = requestMessage;

    return this;
  }

  /// <summary>
  ///   Creates an <see cref="HttpResponseMessage" /> from the settings in this builder.
  /// </summary>
  /// <returns>An <see cref="HttpResponseMessage" />.</returns>
  public HttpResponseMessage Build()
  {
    var message = new HttpResponseMessage(_statusCode)
    {
      ReasonPhrase = _reasonPhrase, RequestMessage = _requestMessage
    };

    if (_content != null)
    {
      message.Content = _content;
    }

    foreach (var keyValuePair in _headers)
    {
      if (keyValuePair.Value.Count > 1)
      {
        message.Headers.Add(keyValuePair.Key, keyValuePair.Value);
      }
      else
      {
        message.Headers.Add(keyValuePair.Key, keyValuePair.Value.First());
      }
    }

    foreach (var keyValuePair in _trailingHeaders)
    {
      if (keyValuePair.Value.Count > 1)
      {
        message.TrailingHeaders.Add(keyValuePair.Key, keyValuePair.Value);
      }
      else
      {
        message.TrailingHeaders.Add(keyValuePair.Key, keyValuePair.Value.First());
      }
    }

    if (_version != null)
    {
      message.Version = _version;
    }

    return message;
  }
}

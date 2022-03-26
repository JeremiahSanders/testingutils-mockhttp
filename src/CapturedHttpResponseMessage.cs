using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Jds.TestingUtils.MockHttp;

/// <summary>
///   A captured <see cref="HttpResponseMessage" />.
/// </summary>
/// <param name="StatusCode">Response <see cref="HttpStatusCode" />.</param>
/// <param name="ReasonPhrase">Response <see cref="HttpResponseMessage.ReasonPhrase" />.</param>
/// <param name="Headers">Response <see cref="HttpResponseHeaders" />.</param>
/// <param name="Version">Response <c>HTTP</c> <see cref="System.Version" />.</param>
/// <param name="TrailingHeaders">
///   Response <see cref="HttpResponseMessage.TrailingHeaders" />
///   <see cref="HttpResponseHeaders" />.
/// </param>
/// <param name="Content">Response <see cref="HttpContent" />.</param>
/// <param name="ContentBytes">Response content, as a <see cref="byte" /> <see cref="Array" />.</param>
/// <param name="ContentString">Response content, as an optional <see cref="string" />.</param>
/// <param name="RequestMessage">Optional response <see cref="CapturedHttpRequestMessage" />.</param>
public record CapturedHttpResponseMessage(
  HttpStatusCode StatusCode,
  string? ReasonPhrase,
  HttpResponseHeaders Headers,
  Version Version,
  HttpResponseHeaders TrailingHeaders,
  HttpContent Content,
  byte[] ContentBytes,
  string? ContentString,
  CapturedHttpRequestMessage? RequestMessage
)
{
  /// <summary>
  ///   Captures the content of <paramref name="message" />.
  /// </summary>
  /// <param name="message">An <see cref="HttpResponseMessage" />.</param>
  /// <param name="cancellationToken">A <see cref="CancellationToken" />.</param>
  /// <returns>A <see cref="CapturedHttpResponseMessage" />.</returns>
  public static async Task<CapturedHttpResponseMessage> FromHttpResponseMessage(HttpResponseMessage message,
    CancellationToken cancellationToken)
  {
    [ExcludeFromCodeCoverage]
    static string? ReadAsString(Encoding encoding, byte[] bytes)
    {
      try
      {
        return encoding.GetString(bytes);
      }
      catch
      {
        return null;
      }
    }

    static Encoding GetEncoding(HttpContentHeaders headers)
    {
      try
      {
        return Encoding.GetEncoding(headers.ContentType?.CharSet ?? string.Empty);
      }
      catch
      {
        return Encoding.UTF8;
      }
    }

    var bytes = await message.Content.ReadAsByteArrayAsync(cancellationToken);
    var content = ReadAsString(GetEncoding(message.Content!.Headers), bytes);

    return new CapturedHttpResponseMessage(
      message.StatusCode,
      message.ReasonPhrase,
      message.Headers,
      message.Version,
      message.TrailingHeaders,
      message.Content,
      bytes,
      content,
      message.RequestMessage == null
        ? null
        : await CapturedHttpRequestMessage.FromHttpRequestMessage(message.RequestMessage, cancellationToken)
    );
  }

  public static HttpResponseMessage ToHttpResponseMessage(CapturedHttpResponseMessage capturedResponse)
  {
    var response = new HttpResponseMessage(capturedResponse.StatusCode)
    {
      Content = new ByteArrayContent(capturedResponse.ContentBytes),
      Version = capturedResponse.Version,
      ReasonPhrase = capturedResponse.ReasonPhrase,
      RequestMessage = capturedResponse.RequestMessage != null
        ? CapturedHttpRequestMessage.ToHttpRequestMessage(capturedResponse.RequestMessage)
        : null
    };
    foreach (var kvp in capturedResponse.Headers)
    {
      response.Headers.Add(kvp.Key, kvp.Value);
    }

    foreach (var kvp in capturedResponse.TrailingHeaders)
    {
      response.TrailingHeaders.Add(kvp.Key, kvp.Value);
    }

    return response;
  }
}

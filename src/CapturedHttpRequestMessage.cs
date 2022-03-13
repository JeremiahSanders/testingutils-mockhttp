using System.Net.Http.Headers;
using System.Text;

namespace Jds.TestingUtils.MockHttp;

/// <summary>
///   A captured <see cref="HttpRequestMessage" />.
/// </summary>
/// <param name="Method">Request <see cref="HttpMethod" />.</param>
/// <param name="RequestUri">Request <see cref="Uri" />.</param>
/// <param name="Headers">Request <see cref="HttpRequestHeaders" />.</param>
/// <param name="Version">Request HTTP message <see cref="Version" />.</param>
/// <param name="VersionPolicy">Request <see cref="HttpVersionPolicy" />.</param>
/// <param name="Options">Request <see cref="HttpRequestOptions" />.</param>
/// <param name="Content">Optional request <see cref="HttpContent" />.</param>
/// <param name="ContentBytes">Optional request content, as a <see cref="byte" /> <see cref="Array" />.</param>
/// <param name="ContentString">Optional request content, as a <see cref="string" />.</param>
public record CapturedHttpRequestMessage(
  HttpMethod Method,
  Uri? RequestUri,
  HttpRequestHeaders Headers,
  Version Version,
  HttpVersionPolicy VersionPolicy,
  HttpRequestOptions Options,
  HttpContent? Content,
  byte[]? ContentBytes,
  string? ContentString
)
{
  /// <summary>
  ///   Captures the content of <paramref name="message" />.
  /// </summary>
  /// <param name="message">An <see cref="HttpRequestMessage" />.</param>
  /// <returns>A <see cref="CapturedHttpRequestMessage" />.</returns>
  public static async Task<CapturedHttpRequestMessage> FromHttpRequestMessage(HttpRequestMessage message)
  {
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

    var bytes = message.Content != null ? await message.Content.ReadAsByteArrayAsync(CancellationToken.None) : null;
    var content = bytes != null ? ReadAsString(GetEncoding(message.Content!.Headers), bytes) : null;

    return new CapturedHttpRequestMessage(
      message.Method,
      message.RequestUri,
      message.Headers,
      message.Version,
      message.VersionPolicy,
      message.Options,
      message.Content,
      bytes,
      content
    );
  }
}

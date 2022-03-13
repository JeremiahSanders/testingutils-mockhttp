using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace Jds.TestingUtils.MockHttp;

/// <summary>
///   Functions which create <see cref="HttpContent" />.
/// </summary>
public static class CreateHttpContent
{
  /// <summary>
  ///   Serializes <paramref name="contentBody" /> as JSON and returns it as <see cref="HttpContent" />.
  /// </summary>
  /// <param name="contentBody">A content body object to serialize.</param>
  /// <param name="jsonSerializerOptions">Optional <see cref="JsonSerializerOptions" />.</param>
  /// <typeparam name="T">A serializable content body object type.</typeparam>
  /// <returns>
  ///   <see cref="HttpContent" />
  /// </returns>
  public static HttpContent ApplicationJson<T>(T contentBody, JsonSerializerOptions? jsonSerializerOptions = null)
  {
    return new StringContent(
      JsonSerializer.Serialize(contentBody, jsonSerializerOptions),
      Encoding.UTF8,
      MediaTypeNames.Application.Json
    );
  }

  /// <summary>
  ///   Create a <see cref="MediaTypeNames.Text.Plain" /> <see cref="HttpContent" />.
  /// </summary>
  /// <param name="content">Body content.</param>
  /// <param name="encoding">Optional <see cref="Encoding" />. Defaults to <see cref="Encoding.UTF8" />.</param>
  /// <returns>
  ///   <see cref="HttpContent" />
  /// </returns>
  public static HttpContent TextPlain(string content, Encoding? encoding = null)
  {
    return new StringContent(content, encoding ?? Encoding.UTF8, MediaTypeNames.Text.Plain);
  }

  /// <inheritdoc cref="ApplicationJson{T}" />
  public static HttpContent ToJsonHttpContent<T>(this T contentBody,
    JsonSerializerOptions? jsonSerializerOptions = null)
  {
    return ApplicationJson(contentBody, jsonSerializerOptions);
  }

  /// <inheritdoc cref="TextPlain" />
  public static HttpContent ToTextHttpContent(this string contentBody, Encoding? encoding = null)
  {
    return TextPlain(contentBody, encoding);
  }
}

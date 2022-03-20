using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;

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
  ///   Serializes <paramref name="contentBody" /> as XML and returns it as <see cref="HttpContent" />.
  /// </summary>
  /// <param name="contentBody">A content body object to serialize.</param>
  /// <param name="contentType">
  ///   An optional <see cref="MediaTypeHeaderValue" />, e.g., <c>application/soap+xml</c>. Defaults
  ///   to <see cref="MediaTypeNames.Application.Xml" />.
  /// </param>
  /// <param name="serializer">An optional <see cref="XmlSerializer" />. If not provided, one will be created.</param>
  /// <typeparam name="T">A serializable content body object type.</typeparam>
  /// <returns>
  ///   <see cref="HttpContent" />
  /// </returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="contentBody" /> is null.</exception>
  /// <exception cref="InvalidOperationException">Thrown when serialization fails.</exception>
  public static HttpContent ApplicationXml<T>(T contentBody,
    MediaTypeHeaderValue? contentType = null,
    XmlSerializer? serializer = null
  )
  {
    if (contentBody == null)
    {
      throw new ArgumentNullException(nameof(contentBody), "Content cannot be null");
    }

    serializer ??= XmlSerialization.GetSerializer(contentBody.GetType());
    contentType ??= new MediaTypeHeaderValue(MediaTypeNames.Application.Xml);

    var writer = new StringWriter();
    serializer.Serialize(writer, contentBody);
    writer.Flush();
    return new StringContent(writer.ToString(), writer.Encoding) { Headers = { ContentType = contentType } };
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

  /// <inheritdoc cref="ApplicationXml{T}" />
  public static HttpContent ToXmlHttpContent<T>(this T contentBody,
    MediaTypeHeaderValue? contentType = null,
    XmlSerializer? serializer = null
  )
  {
    return ApplicationXml(contentBody, contentType, serializer);
  }
}

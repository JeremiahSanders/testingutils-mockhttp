using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace Jds.TestingUtils.MockHttp;

public static partial class MessageCaseHandlerBuilderExtensions
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
  public static HttpContent ToJsonHttpContent<T>(this T contentBody,
    JsonSerializerOptions? jsonSerializerOptions = null)
  {
    return new StringContent(
      JsonSerializer.Serialize(contentBody, jsonSerializerOptions),
      Encoding.UTF8,
      MediaTypeNames.Application.Json
    );
  }

  /// <summary>
  ///   Arranges the <see cref="MessageCaseHandler" /> to respond with a JSON representation of
  ///   <paramref name="bodyObject" />.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" />.</param>
  /// <param name="httpStatusCode">A response status code.</param>
  /// <param name="bodyObject">A predefined object to be JSON serialized.</param>
  /// <param name="jsonSerializerOptions">Optional <see cref="JsonSerializerOptions" /> configuring serialization.</param>
  /// <typeparam name="T">The response body object type.</typeparam>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  public static MessageCaseHandlerBuilder RespondContentJson<T>(this MessageCaseHandlerBuilder builder,
    HttpStatusCode httpStatusCode,
    T bodyObject,
    JsonSerializerOptions? jsonSerializerOptions = null
  )
  {
    return builder.SetResponseHandler((_, _) =>
      new HttpResponseMessage(httpStatusCode) { Content = bodyObject.ToJsonHttpContent(jsonSerializerOptions) }.AsTask()
    );
  }

  /// <summary>
  ///   Arranges the <see cref="MessageCaseHandler" /> to respond with a JSON object derived using
  ///   <paramref name="deriveBody" />.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" />.</param>
  /// <param name="deriveHttpStatusCode">A <see cref="Func{T1, T2, TResult}" /> which will return a response status code.</param>
  /// <param name="deriveBody">A <see cref="Func{T1, T2, TResult}" /> which will create a serializable response body object.</param>
  /// <param name="jsonSerializerOptions">Optional <see cref="JsonSerializerOptions" />.</param>
  /// <typeparam name="T">The response body object type.</typeparam>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  public static MessageCaseHandlerBuilder RespondDerivedContentJson<T>(this MessageCaseHandlerBuilder builder,
    Func<HttpRequestMessage, CancellationToken, Task<HttpStatusCode>> deriveHttpStatusCode,
    Func<HttpRequestMessage, CancellationToken, Task<T>> deriveBody,
    JsonSerializerOptions? jsonSerializerOptions = null
  )
  {
    async Task<HttpResponseMessage> ReturnHandler(HttpRequestMessage message, CancellationToken cancellationToken)
    {
      return new HttpResponseMessage(await deriveHttpStatusCode(message, cancellationToken))
      {
        Content = (await deriveBody(message, cancellationToken)).ToJsonHttpContent(jsonSerializerOptions)
      };
    }

    return builder.SetResponseHandler(ReturnHandler);
  }

  /// <summary>
  ///   Arranges the <see cref="MessageCaseHandler" /> to respond with a JSON object derived using
  ///   <paramref name="deriveBody" />, from the incoming message body, parsed as a <typeparamref name="TRequestBody" />.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" />.</param>
  /// <param name="deriveHttpStatusCode">A <see cref="Func{T, TResult}" /> which will return a response status code.</param>
  /// <param name="deriveBody">A <see cref="Func{T1, T2, TResult}" /> which will create a serializable response body object.</param>
  /// <param name="defaultRequest">
  ///   A default <typeparamref name="TRequestBody" />, used when the incoming request body cannot
  ///   be deserialized.
  /// </param>
  /// <param name="jsonSerializerOptions">Optional <see cref="JsonSerializerOptions" />.</param>
  /// <typeparam name="TRequestBody">The request body object type.</typeparam>
  /// <typeparam name="TResponseBody">The response body object type.</typeparam>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  public static MessageCaseHandlerBuilder RespondDerivedContentJson<TRequestBody, TResponseBody>(
    this MessageCaseHandlerBuilder builder,
    Func<TRequestBody, CancellationToken, Task<HttpStatusCode>> deriveHttpStatusCode,
    Func<TRequestBody, CancellationToken, Task<TResponseBody>> deriveBody,
    TRequestBody defaultRequest,
    JsonSerializerOptions? jsonSerializerOptions = null
  )
  {
    async Task<HttpResponseMessage> ReturnHandler(HttpRequestMessage message, CancellationToken cancellationToken)
    {
      TRequestBody requestBody;
      try
      {
        if (message.Content == null)
        {
          requestBody = defaultRequest;
        }
        else
        {
          await using var contentStream = await message.Content.ReadAsStreamAsync(cancellationToken);
          requestBody = JsonSerializer.Deserialize<TRequestBody>(contentStream) ?? defaultRequest;
        }
      }
      catch
      {
        requestBody = defaultRequest;
      }

      return new HttpResponseMessage(await deriveHttpStatusCode(requestBody, cancellationToken))
      {
        Content = (await deriveBody(requestBody, cancellationToken)).ToJsonHttpContent(jsonSerializerOptions)
      };
    }

    return builder.SetResponseHandler(ReturnHandler);
  }

  /// <summary>
  ///   Arranges the <see cref="MessageCaseHandler" /> to respond with a JSON object derived using
  ///   <paramref name="deriveBody" />, from the incoming message body, parsed as a <typeparamref name="TRequestBody" />.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" />.</param>
  /// <param name="deriveHttpStatusCode">A <see cref="Func{T, TResult}" /> which will return a response status code.</param>
  /// <param name="deriveBody">A <see cref="Func{T, TResult}" /> which will create a serializable response body object.</param>
  /// <param name="defaultRequest">
  ///   A default <typeparamref name="TRequestBody" />, used when the incoming request body cannot
  ///   be deserialized.
  /// </param>
  /// <param name="jsonSerializerOptions">Optional <see cref="JsonSerializerOptions" />.</param>
  /// <typeparam name="TRequestBody">The request body object type.</typeparam>
  /// <typeparam name="TResponseBody">The response body object type.</typeparam>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  public static MessageCaseHandlerBuilder RespondDerivedContentJson<TRequestBody, TResponseBody>(
    this MessageCaseHandlerBuilder builder,
    Func<TRequestBody, HttpStatusCode> deriveHttpStatusCode,
    Func<TRequestBody, TResponseBody> deriveBody,
    TRequestBody defaultRequest,
    JsonSerializerOptions? jsonSerializerOptions = null
  )
  {
    async Task<HttpResponseMessage> ReturnHandler(HttpRequestMessage message, CancellationToken cancellationToken)
    {
      TRequestBody requestBody;
      try
      {
        if (message.Content == null)
        {
          requestBody = defaultRequest;
        }
        else
        {
          await using var contentStream = await message.Content.ReadAsStreamAsync(cancellationToken);
          requestBody = JsonSerializer.Deserialize<TRequestBody>(contentStream) ?? defaultRequest;
        }
      }
      catch
      {
        requestBody = defaultRequest;
      }

      return new HttpResponseMessage(deriveHttpStatusCode(requestBody))
      {
        Content = deriveBody(requestBody).ToJsonHttpContent(jsonSerializerOptions)
      };
    }

    return builder.SetResponseHandler(ReturnHandler);
  }
}

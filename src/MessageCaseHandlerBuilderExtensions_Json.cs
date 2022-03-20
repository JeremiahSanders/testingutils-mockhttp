using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;

namespace Jds.TestingUtils.MockHttp;

public static partial class MessageCaseHandlerBuilderExtensions
{
  /// <summary>
  ///   Arranges the <see cref="MessageCaseHandler" /> to accept requests based upon their <see cref="HttpMethod" />,
  ///   <see cref="Uri" />, and request body, parsed as <typeparamref name="T" /> from JSON.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" />.</param>
  /// <param name="rule">A <see cref="Func{T1,T2,T3,TResult}" /> which evaluates the request for acceptance.</param>
  /// <param name="defaultRequestBody">A default <typeparamref name="T" />.</param>
  /// <param name="serializerOptions">Optional <see cref="JsonSerializerOptions" /> configuring deserialization.</param>
  /// <typeparam name="T">A request body type.</typeparam>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  public static MessageCaseHandlerBuilder AcceptRouteJson<T>(this MessageCaseHandlerBuilder builder,
    Func<HttpMethod, Uri?, T, bool> rule,
    T defaultRequestBody,
    JsonSerializerOptions? serializerOptions = null
  )
  {
    return builder.AddAcceptRule(message =>
    {
      try
      {
        var requestBody = (message.ContentString != null
                            ? JsonSerializer.Deserialize<T>(message.ContentString, serializerOptions)
                            : defaultRequestBody)
                          ?? defaultRequestBody;
        return Task.FromResult(rule(message.Method, message.RequestUri, requestBody));
      }
      catch
      {
        return Task.FromResult(rule(message.Method, message.RequestUri, defaultRequestBody));
      }
    });
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
    Func<CapturedHttpRequestMessage, CancellationToken, Task<HttpStatusCode>> deriveHttpStatusCode,
    Func<CapturedHttpRequestMessage, CancellationToken, Task<T>> deriveBody,
    JsonSerializerOptions? jsonSerializerOptions = null
  )
  {
    async Task<HttpResponseMessage> ReturnHandler(CapturedHttpRequestMessage message,
      CancellationToken cancellationToken)
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
    return builder.RespondWith(async (responseBuilder, message, cancellationToken) =>
    {
      var requestBody = await message.SafelyDeserialize(defaultRequest, cancellationToken, jsonSerializerOptions);

      return responseBuilder
        .WithStatusCode(await deriveHttpStatusCode(requestBody, cancellationToken))
        .WithContent(
          (await deriveBody(requestBody, cancellationToken)).ToJsonHttpContent(jsonSerializerOptions));
    });
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
    return builder.RespondWith(async (responseBuilder, message, cancellationToken) =>
    {
      var requestBody = await message.SafelyDeserialize(defaultRequest, cancellationToken, jsonSerializerOptions);

      return responseBuilder
        .WithStatusCode(deriveHttpStatusCode(requestBody))
        .WithContent(deriveBody(requestBody).ToJsonHttpContent(jsonSerializerOptions));
    });
  }

  [SuppressMessage("ReSharper", "UnusedParameter.Local")]
  private static Task<TRequestBody> SafelyDeserialize<TRequestBody>(
    this CapturedHttpRequestMessage message,
    TRequestBody defaultRequest,
    CancellationToken cancellationToken,
    JsonSerializerOptions? jsonSerializerOptions = null
  )
  {
    TRequestBody requestBody;
    try
    {
      if (message.ContentString == null)
      {
        requestBody = defaultRequest;
      }
      else
      {
        requestBody = JsonSerializer.Deserialize<TRequestBody>(message.ContentString, jsonSerializerOptions) ??
                      defaultRequest;
      }
    }
    catch
    {
      requestBody = defaultRequest;
    }

    return Task.FromResult(requestBody);
  }
}

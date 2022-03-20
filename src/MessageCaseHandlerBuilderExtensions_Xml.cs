using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Xml.Serialization;

namespace Jds.TestingUtils.MockHttp;

public static partial class MessageCaseHandlerBuilderExtensions
{
  /// <summary>
  ///   Arranges the <see cref="MessageCaseHandler" /> to accept requests based upon their <see cref="HttpMethod" />,
  ///   <see cref="Uri" />, and request body, parsed as <typeparamref name="T" /> from XML.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" />.</param>
  /// <param name="rule">A <see cref="Func{T1,T2,T3,TResult}" /> which evaluates the request for acceptance.</param>
  /// <param name="defaultRequestBody">A default <typeparamref name="T" />.</param>
  /// <param name="xmlSerializer">Optional <see cref="XmlSerializer" /> providing deserialization.</param>
  /// <typeparam name="T">A request body type.</typeparam>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  public static MessageCaseHandlerBuilder AcceptRouteXml<T>(this MessageCaseHandlerBuilder builder,
    Func<HttpMethod, Uri?, T, bool> rule,
    T defaultRequestBody,
    XmlSerializer? xmlSerializer = null
  )
  {
    return builder.AddAcceptRule(message =>
    {
      var requestBody = message.SafelyDeserialize(defaultRequestBody, xmlSerializer);
      return Task.FromResult(rule(message.Method, message.RequestUri, requestBody));
    });
  }

  /// <summary>
  ///   Arranges the <see cref="MessageCaseHandler" /> to respond with a XML representation of
  ///   <paramref name="bodyObject" />.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" />.</param>
  /// <param name="httpStatusCode">A response status code.</param>
  /// <param name="bodyObject">A predefined object to be XML serialized.</param>
  /// <param name="contentType"></param>
  /// <param name="xmlSerializer">Optional <see cref="XmlSerializer" /> configuring serialization.</param>
  /// <typeparam name="T">The response body object type.</typeparam>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  public static MessageCaseHandlerBuilder RespondContentXml<T>(this MessageCaseHandlerBuilder builder,
    HttpStatusCode httpStatusCode,
    T bodyObject,
    MediaTypeHeaderValue? contentType = null,
    XmlSerializer? xmlSerializer = null)
  {
    return builder.SetResponseHandler((_, _) =>
      new HttpResponseMessage(httpStatusCode) { Content = bodyObject.ToXmlHttpContent(contentType, xmlSerializer) }
        .AsTask()
    );
  }

  /// <summary>
  ///   Arranges the <see cref="MessageCaseHandler" /> to respond with a XML object derived using
  ///   <paramref name="deriveBody" />.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" />.</param>
  /// <param name="deriveHttpStatusCode">A <see cref="Func{T1, T2, TResult}" /> which will return a response status code.</param>
  /// <param name="deriveBody">A <see cref="Func{T1, T2, TResult}" /> which will create a serializable response body object.</param>
  /// <param name="contentType"></param>
  /// <param name="xmlSerializer">Optional <see cref="XmlSerializer" />.</param>
  /// <typeparam name="T">The response body object type.</typeparam>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  public static MessageCaseHandlerBuilder RespondDerivedContentXml<T>(this MessageCaseHandlerBuilder builder,
    Func<CapturedHttpRequestMessage, CancellationToken, Task<HttpStatusCode>> deriveHttpStatusCode,
    Func<CapturedHttpRequestMessage, CancellationToken, Task<T>> deriveBody,
    MediaTypeHeaderValue? contentType = null,
    XmlSerializer? xmlSerializer = null)
  {
    async Task<HttpResponseMessage> ReturnHandler(CapturedHttpRequestMessage message,
      CancellationToken cancellationToken)
    {
      return new HttpResponseMessage(await deriveHttpStatusCode(message, cancellationToken))
      {
        Content = (await deriveBody(message, cancellationToken)).ToXmlHttpContent(contentType, xmlSerializer)
      };
    }

    return builder.SetResponseHandler(ReturnHandler);
  }

  /// <summary>
  ///   Arranges the <see cref="MessageCaseHandler" /> to respond with a XML object derived using
  ///   <paramref name="deriveBody" />, from the incoming message body, parsed as a <typeparamref name="TRequestBody" />.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" />.</param>
  /// <param name="deriveHttpStatusCode">A <see cref="Func{T, TResult}" /> which will return a response status code.</param>
  /// <param name="deriveBody">A <see cref="Func{T1, T2, TResult}" /> which will create a serializable response body object.</param>
  /// <param name="defaultRequest">
  ///   A default <typeparamref name="TRequestBody" />, used when the incoming request body cannot
  ///   be deserialized.
  /// </param>
  /// <param name="contentType"></param>
  /// <param name="xmlSerializer">Optional <see cref="XmlSerializer" />.</param>
  /// <typeparam name="TRequestBody">The request body object type.</typeparam>
  /// <typeparam name="TResponseBody">The response body object type.</typeparam>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  public static MessageCaseHandlerBuilder RespondDerivedContentXml<TRequestBody, TResponseBody>(
    this MessageCaseHandlerBuilder builder,
    Func<TRequestBody, CancellationToken, Task<HttpStatusCode>> deriveHttpStatusCode,
    Func<TRequestBody, CancellationToken, Task<TResponseBody>> deriveBody,
    TRequestBody defaultRequest,
    MediaTypeHeaderValue? contentType = null,
    XmlSerializer? xmlSerializer = null)
  {
    return builder.RespondWith(async (responseBuilder, message, cancellationToken) =>
    {
      var requestBody = message.SafelyDeserialize(defaultRequest, xmlSerializer);

      return responseBuilder
        .WithStatusCode(await deriveHttpStatusCode(requestBody, cancellationToken))
        .WithContent(
          (await deriveBody(requestBody, cancellationToken)).ToXmlHttpContent(contentType, xmlSerializer));
    });
  }

  /// <summary>
  ///   Arranges the <see cref="MessageCaseHandler" /> to respond with a XML object derived using
  ///   <paramref name="deriveBody" />, from the incoming message body, parsed as a <typeparamref name="TRequestBody" />.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" />.</param>
  /// <param name="deriveHttpStatusCode">A <see cref="Func{T, TResult}" /> which will return a response status code.</param>
  /// <param name="deriveBody">A <see cref="Func{T, TResult}" /> which will create a serializable response body object.</param>
  /// <param name="defaultRequest">
  ///   A default <typeparamref name="TRequestBody" />, used when the incoming request body cannot
  ///   be deserialized.
  /// </param>
  /// <param name="contentType"></param>
  /// <param name="xmlSerializer">Optional <see cref="XmlSerializer" />.</param>
  /// <typeparam name="TRequestBody">The request body object type.</typeparam>
  /// <typeparam name="TResponseBody">The response body object type.</typeparam>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  public static MessageCaseHandlerBuilder RespondDerivedContentXml<TRequestBody, TResponseBody>(
    this MessageCaseHandlerBuilder builder,
    Func<TRequestBody, HttpStatusCode> deriveHttpStatusCode,
    Func<TRequestBody, TResponseBody> deriveBody,
    TRequestBody defaultRequest,
    MediaTypeHeaderValue? contentType = null,
    XmlSerializer? xmlSerializer = null)
  {
    return builder.RespondWith((responseBuilder, message, _) =>
    {
      var requestBody = message.SafelyDeserialize(defaultRequest, xmlSerializer);

      return Task.FromResult(responseBuilder
        .WithStatusCode(deriveHttpStatusCode(requestBody))
        .WithContent(deriveBody(requestBody).ToXmlHttpContent(contentType, xmlSerializer)));
    });
  }

  [SuppressMessage("ReSharper", "UnusedParameter.Local")]
  private static TRequestBody SafelyDeserialize<TRequestBody>(
    this CapturedHttpRequestMessage message,
    TRequestBody defaultRequest,
    XmlSerializer? xmlSerializer = null
  )
  {
    TRequestBody requestBody;
    try
    {
      if (message.ContentBytes == null)
      {
        requestBody = defaultRequest;
      }
      else
      {
        xmlSerializer ??= XmlSerialization.GetSerializer(typeof(TRequestBody));
        requestBody =
          (TRequestBody?)xmlSerializer.Deserialize(new MemoryStream(message.ContentBytes ?? Array.Empty<byte>())) ??
          defaultRequest;
      }
    }
    catch (Exception exception)
    {
      requestBody = defaultRequest;
    }

    return requestBody;
  }
}

using System.Net;

namespace Jds.TestingUtils.MockHttp;

/// <summary>
///   Methods extending <see cref="MessageCaseHandlerBuilder" />.
/// </summary>
public static partial class MessageCaseHandlerBuilderExtensions
{
  private static Task<T> AsTask<T>(this T value)
  {
    return Task.FromResult(value);
  }

  /// <summary>
  ///   Set the response to a static <see cref="HttpStatusCode" /> and <see cref="HttpContent" />.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" />.</param>
  /// <param name="httpStatusCode">A <see cref="HttpStatusCode" />.</param>
  /// <param name="httpContent">A <see cref="HttpContent" />.</param>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  public static MessageCaseHandlerBuilder RespondStaticContent(this MessageCaseHandlerBuilder builder,
    HttpStatusCode httpStatusCode, HttpContent httpContent)
  {
    return builder.SetResponseHandler((_, _) =>
      new HttpResponseMessage(httpStatusCode) { Content = httpContent }.AsTask()
    );
  }

  /// <summary>
  ///   Set the response to a static <see cref="HttpStatusCode" />.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" />.</param>
  /// <param name="httpStatusCode">A <see cref="HttpStatusCode" />.</param>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  public static MessageCaseHandlerBuilder RespondStatusCode(this MessageCaseHandlerBuilder builder,
    HttpStatusCode httpStatusCode)
  {
    return builder.SetResponseHandler((_, _) =>
      new HttpResponseMessage(httpStatusCode).AsTask()
    );
  }

  /// <summary>
  ///   Configures the <see cref="MessageCaseHandler" /> to build its responses using a
  ///   <see cref="HttpResponseMessageBuilder" />, which exposes a fluent arrangement API.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" />.</param>
  /// <param name="handlerBuilder">
  ///   A <see cref="Func{T1,T2,TResult}" /> which configures the
  ///   <see cref="MessageCaseHandler.HandleMessage" />.
  /// </param>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  public static MessageCaseHandlerBuilder RespondWith(this MessageCaseHandlerBuilder builder,
    Func<HttpResponseMessageBuilder, CapturedHttpRequestMessage, HttpResponseMessageBuilder> handlerBuilder)
  {
    return builder.SetResponseHandler((message, _) =>
      handlerBuilder(new HttpResponseMessageBuilder()
          .WithRequestMessage(message.ToHttpRequestMessage()), message)
        .Build()
        .AsTask()
    );
  }

  /// <summary>
  ///   Configures the <see cref="MessageCaseHandler" /> to build its responses using a
  ///   <see cref="HttpResponseMessageBuilder" />, which exposes a fluent arrangement API.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" />.</param>
  /// <param name="handlerBuilder">
  ///   A <see cref="Func{T1,T2,T3,TResult}" /> which configures the
  ///   <see cref="MessageCaseHandler.HandleMessage" />.
  /// </param>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  public static MessageCaseHandlerBuilder RespondWith(this MessageCaseHandlerBuilder builder,
    Func<HttpResponseMessageBuilder, CapturedHttpRequestMessage, CancellationToken, Task<HttpResponseMessageBuilder>>
      handlerBuilder)
  {
    return builder.SetResponseHandler(async (message, cancellationToken) =>
      (await handlerBuilder(new HttpResponseMessageBuilder()
        .WithRequestMessage(message.ToHttpRequestMessage()), message, cancellationToken))
      .Build());
  }

  private static HttpRequestMessage ToHttpRequestMessage(this CapturedHttpRequestMessage capturedRequest)
  {
    var message = new HttpRequestMessage(capturedRequest.Method, capturedRequest.RequestUri)
    {
      Content = capturedRequest.Content,
      Method = capturedRequest.Method,
      Version = capturedRequest.Version,
      RequestUri = capturedRequest.RequestUri,
      VersionPolicy = capturedRequest.VersionPolicy
    };

    foreach (var (key, values) in capturedRequest.Headers)
    {
      message.Headers.Add(key, values);
    }

    foreach (var keyValuePair in capturedRequest.Options)
    {
      message.Options.Set(new HttpRequestOptionsKey<object?>(keyValuePair.Key), keyValuePair.Value);
    }

    return message;
  }
}

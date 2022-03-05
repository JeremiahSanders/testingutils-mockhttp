namespace Jds.TestingUtils.MockHttp;

public static partial class MessageCaseHandlerBuilderExtensions
{
  /// <summary>
  ///   Arranges the <see cref="MessageCaseHandler" /> to accept all requests.
  /// </summary>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  public static MessageCaseHandlerBuilder AcceptAll(this MessageCaseHandlerBuilder builder)
  {
    return builder.WithAcceptRule(_ => true.AsTask());
  }

  /// <summary>
  ///   Arranges the <see cref="MessageCaseHandler" /> to accept all requests matching <paramref name="httpMethod" />.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" /> to extend.</param>
  /// <param name="httpMethod">An accepted <see cref="HttpMethod" />.</param>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  public static MessageCaseHandlerBuilder AcceptMethod(this MessageCaseHandlerBuilder builder, HttpMethod httpMethod)
  {
    return builder.WithAcceptRule(message => (message.Method == httpMethod).AsTask());
  }

  /// <summary>
  ///   Arranges the <see cref="MessageCaseHandler" /> to accept requests for which <paramref name="matcher" /> returns true.
  /// </summary>
  /// <param name="builder"></param>
  /// <param name="matcher"></param>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  public static MessageCaseHandlerBuilder AcceptRoute(this MessageCaseHandlerBuilder builder,
    Func<HttpMethod, Uri?, bool> matcher)
  {
    return builder.WithAcceptRule(message => matcher(message.Method, message.RequestUri).AsTask());
  }

  /// <summary>
  ///   Arranges the <see cref="MessageCaseHandler" /> to accept all requests with <see cref="HttpRequestMessage.Method" />
  ///   equal to <paramref name="httpMethod" /> and <see cref="HttpRequestMessage.RequestUri" /> equal to
  ///   <paramref name="uri" />.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" /> to extend.</param>
  /// <param name="httpMethod"></param>
  /// <param name="uri"></param>
  /// <returns></returns>
  public static MessageCaseHandlerBuilder AcceptRoute(this MessageCaseHandlerBuilder builder, HttpMethod httpMethod,
    Uri uri)
  {
    return builder.AcceptRoute((requestMethod, requestUri) => requestMethod == httpMethod && requestUri == uri);
  }

  /// <summary>
  ///   Arranges the <see cref="MessageCaseHandler" /> to accept requests for which <paramref name="matcher" /> returns true.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" /> to extend.</param>
  /// <param name="matcher"></param>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  public static MessageCaseHandlerBuilder AcceptUri(this MessageCaseHandlerBuilder builder, Func<Uri?, bool> matcher)
  {
    return builder.WithAcceptRule(message => matcher(message.RequestUri).AsTask());
  }

  /// <summary>
  ///   Arranges the <see cref="MessageCaseHandler" /> to accept all requests with
  ///   <see cref="HttpRequestMessage.RequestUri" /> equal to <paramref name="uriToAccept" />.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" /> to extend.</param>
  /// <param name="uriToAccept">An accepted <see cref="Uri" />.</param>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  public static MessageCaseHandlerBuilder AcceptUri(this MessageCaseHandlerBuilder builder, Uri uriToAccept)
  {
    return builder.AcceptUri(uri => uri == uriToAccept);
  }
}

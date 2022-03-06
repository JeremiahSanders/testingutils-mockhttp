using System.Collections.Concurrent;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json.Serialization;

namespace Jds.TestingUtils.MockHttp.Tests.Unit;

internal static class MockApi
{
  public static readonly Uri BaseUri = new("https://not-real", UriKind.Absolute);
  public static readonly string PlainTextGetBody = "This is some plain text.";
  public static Uri PlainTextGetRoute { get; } = new(BaseUri, "plaintext");
  public static Uri SumIntsJsonPostRoute { get; } = new(BaseUri, "sum");
  public static Uri StatefulGetRoute { get; } = new(BaseUri, "stateful");
  public static Uri StatefulAddPostRoute { get; } = new(BaseUri, "stateful/add");
  public static Uri StatefulRemovePostRoute { get; } = new(BaseUri, "stateful/remove");

  /// <summary>
  ///   Create an <see cref="HttpClient" /> prearranged to simulate a mock API.
  /// </summary>
  /// <remarks>
  ///   <para>The following APIs are supported:</para>
  ///   <para>
  ///     <c>* https://not-real/</c> returns <see cref="HttpStatusCode.OK" /> to all requests and includes multiple custom
  ///     headers.
  ///   </para>
  ///   <para>
  ///     <c>HEAD https://not-real/plaintext</c> returns <see cref="HttpStatusCode.OK" />.
  ///   </para>
  ///   <para>
  ///     <c>GET https://not-real/plaintext</c> returns <see cref="HttpStatusCode.OK" /> and a plain text body,
  ///     <see cref="PlainTextGetBody" />.
  ///   </para>
  ///   <para>
  ///     <c>POST https://not-real/sum</c> expects a <see cref="SumIntsJsonRequest" /> JSON request body, and returns
  ///     <see cref="HttpStatusCode.OK" /> with a <see cref="SumIntsJsonResponse" /> JSON body.
  ///   </para>
  ///   <para>
  ///     <c>GET https://not-real/stateful</c> returns <see cref="HttpStatusCode.OK" /> with an int array JSON body.
  ///   </para>
  ///   <para>
  ///     <c>POST https://not-real/stateful/add</c> expects a <see cref="StatefulRequest" /> JSON request body, and returns
  ///     <see cref="HttpStatusCode.OK" />. Values are added to <c>GET https://not-real/stateful</c>.
  ///   </para>
  ///   <para>
  ///     <c>POST https://not-real/stateful/remove</c> expects a <see cref="StatefulRequest" /> JSON request body, and
  ///     returns <see cref="HttpStatusCode.OK" />. Values are removed from <c>GET https://not-real/stateful</c>.
  ///   </para>
  /// </remarks>
  /// <returns>A mocked <see cref="HttpClient" />.</returns>
  public static HttpClient CreateCompleteApi()
  {
    ConcurrentBag<int> state = new();

    return new MockHttpBuilder()
      .WithHandler(RootRoute)
      .WithHandler(PlaintextHead)
      .WithHandler(PlaintextGet)
      .WithHandler(SumPost)
      .WithHandler(builder => StatefulGet(builder, state))
      .WithHandler(builder => StatefulAddPost(builder, state))
      .WithHandler(builder => StatefulRemovePost(builder, state))
      .BuildHttpClient();
  }

  /// <summary>
  ///   Arranges <paramref name="builder" /> to accept all requests for <see cref="BaseUri" />,
  ///   returning <see cref="HttpStatusCode.OK" /> and multiple custom headers.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" />.</param>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  private static MessageCaseHandlerBuilder RootRoute(MessageCaseHandlerBuilder builder)
  {
    return builder.AcceptUri(BaseUri)
      .RespondWith((responseBuilder, message) =>
        responseBuilder.WithStatusCode(HttpStatusCode.OK)
          .WithHeader("custom-header", "custom-header singular value")
          .WithHeader("multi-item-header", "multi-item-header value 1")
          .WithHeader("multi-item-header", "multi-item-header value 2")
          .WithHeader("multi-item-header", "multi-item-header value 3")
          .WithTrailingHeader("custom-trailing-header", "custom-trailing-header singular value")
          .WithTrailingHeader("multi-item-trailing-header", "multi-item-trailing-header value 1")
          .WithTrailingHeader("multi-item-trailing-header", "multi-item-trailing-header value 2")
          .WithTrailingHeader("multi-item-trailing-header", "multi-item-trailing-header value 3")
          .WithVersion(new Version(2, 1, 3))
          .WithReasonPhrase("OK")
          .WithContent(new StringContent($"Response to uri: {message.RequestUri}", Encoding.UTF8,
            MediaTypeNames.Text.Plain))
      );
  }

  /// <summary>
  ///   Arranges <paramref name="builder" /> to accept a <see cref="HttpMethod.Head" /> <see cref="PlainTextGetRoute" />,
  ///   returning <see cref="HttpStatusCode.OK" />.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" />.</param>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  private static MessageCaseHandlerBuilder PlaintextHead(MessageCaseHandlerBuilder builder)
  {
    return builder.AcceptRoute(HttpMethod.Head, PlainTextGetRoute).RespondStatusCode(HttpStatusCode.OK);
  }

  /// <summary>
  ///   Arranges <paramref name="builder" /> to accept a <see cref="HttpMethod.Get" /> <see cref="PlainTextGetRoute" />,
  ///   returning <see cref="HttpStatusCode.OK" /> with a plain text <see cref="PlainTextGetBody" /> body.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" />.</param>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  private static MessageCaseHandlerBuilder PlaintextGet(MessageCaseHandlerBuilder builder)
  {
    return builder.AcceptRoute(HttpMethod.Get, PlainTextGetRoute)
      .RespondStaticContent(
        HttpStatusCode.OK,
        new StringContent(PlainTextGetBody, Encoding.UTF8)
      );
  }

  /// <summary>
  ///   Arranges <paramref name="builder" /> to sum the values sent in a <see cref="SumIntsJsonRequest" /> sent to
  ///   <see cref="HttpMethod.Post" /> <see cref="SumIntsJsonPostRoute" />, returning a <see cref="SumIntsJsonResponse" />.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" />.</param>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  private static MessageCaseHandlerBuilder SumPost(MessageCaseHandlerBuilder builder)
  {
    return builder.AcceptRoute(HttpMethod.Post, SumIntsJsonPostRoute)
      .RespondDerivedContentJson(
        (_, _) => Task.FromResult(HttpStatusCode.OK),
        (sumIntsRequest, _) =>
          Task.FromResult(new SumIntsJsonResponse { Sum = sumIntsRequest.Ints.Sum() }), new SumIntsJsonRequest()
      );
  }

  /// <summary>
  ///   Arranges <paramref name="builder" /> to return a JSON int array when receiving a GET <see cref="StatefulGetRoute" />.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" />.</param>
  /// <param name="statefulStore">
  ///   A <see cref="ConcurrentBag{T}" /> which stores the persistent <see cref="int" />
  ///   collection throughout multiple requests.
  /// </param>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  private static MessageCaseHandlerBuilder StatefulGet(MessageCaseHandlerBuilder builder,
    ConcurrentBag<int> statefulStore)
  {
    return builder.AcceptRoute(HttpMethod.Get, StatefulGetRoute)
      .RespondWith((caseBuilder, _) =>
        caseBuilder
          .WithStatusCode(HttpStatusCode.OK)
          .WithContent(statefulStore.ToJsonHttpContent())
      );
  }

  /// <summary>
  ///   Arranges <paramref name="builder" /> to add an <see cref="int" /> to <paramref name="statefulStore" /> when receiving
  ///   a POST <see cref="StatefulAddPostRoute" />.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" />.</param>
  /// <param name="statefulStore">
  ///   A <see cref="ConcurrentBag{T}" /> which stores the persistent <see cref="int" />
  ///   collection throughout multiple requests.
  /// </param>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  private static MessageCaseHandlerBuilder StatefulAddPost(MessageCaseHandlerBuilder builder,
    ConcurrentBag<int> statefulStore)
  {
    return builder.AcceptRoute(HttpMethod.Post, StatefulAddPostRoute)
      .RespondDerivedContentJson(
        valueDto => valueDto.Value.HasValue ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
        valueDto =>
        {
          if (valueDto.Value == null)
          {
            return "Value is required";
          }

          statefulStore.Add(valueDto.Value ?? 0);
          return "Added";
        },
        new StatefulRequest { Value = 0 }
      );
  }

  /// <summary>
  ///   Arranges <paramref name="builder" /> to remove an <see cref="int" /> from <paramref name="statefulStore" /> when
  ///   receiving a POST <see cref="StatefulRemovePostRoute" />.
  /// </summary>
  /// <param name="builder">A <see cref="MessageCaseHandlerBuilder" />.</param>
  /// <param name="statefulStore">
  ///   A <see cref="ConcurrentBag{T}" /> which stores the persistent <see cref="int" />
  ///   collection throughout multiple requests.
  /// </param>
  /// <returns>
  ///   <paramref name="builder" />
  /// </returns>
  private static MessageCaseHandlerBuilder StatefulRemovePost(MessageCaseHandlerBuilder builder,
    ConcurrentBag<int> statefulStore)
  {
    return builder.AcceptRoute(HttpMethod.Post, StatefulRemovePostRoute)
      .RespondDerivedContentJson(
        valueDto => valueDto.Value.HasValue ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
        valueDto =>
        {
          if (valueDto.Value == null)
          {
            return "Value is required";
          }

          var currentList = statefulStore.ToList();
          statefulStore.Clear();
          foreach (var value in currentList.Except(new[] { valueDto.Value ?? 0 }))
          {
            statefulStore.Add(value);
          }

          return "Removed";
        },
        new StatefulRequest { Value = 0 }
      );
  }

  public record StatefulRequest
  {
    [JsonPropertyName("value")]
    public int? Value { get; init; }
  }

  public record SumIntsJsonRequest
  {
    [JsonPropertyName("ints")]
    public int[] Ints { get; init; } = Array.Empty<int>();
  }

  public record SumIntsJsonResponse
  {
    [JsonPropertyName("sum")]
    public int Sum { get; init; }
  }
}

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

  public static HttpClient CreateCompleteApi()
  {
    // _ALL_ https://not-real/
    // HEAD https://not-real/plaintext
    // GET https://not-real/plaintext
    // POST https://not-real/sum

    return new MockHttpBuilder()
      .WithHandler(RootRoute)
      .WithHandler(HeadPlaintext)
      .WithHandler(GetPlaintext)
      .WithHandler(PostSum)
      .BuildHttpClient();
  }

  private static MessageCaseHandlerBuilder PostSum(MessageCaseHandlerBuilder builder)
  {
    return builder.AcceptRoute(HttpMethod.Post, SumIntsJsonPostRoute)
      .RespondDerivedContentJson(
        HttpStatusCode.OK,
        (sumIntsRequest, _) =>
          Task.FromResult(new SumIntsJsonResponse { Sum = sumIntsRequest.Ints.Sum() }), new SumIntsJsonRequest()
      );
  }

  private static MessageCaseHandlerBuilder GetPlaintext(MessageCaseHandlerBuilder builder)
  {
    return builder.AcceptRoute(HttpMethod.Get, PlainTextGetRoute)
      .RespondStaticContent(
        HttpStatusCode.OK,
        new StringContent(PlainTextGetBody, Encoding.UTF8)
      );
  }

  private static MessageCaseHandlerBuilder RootRoute(MessageCaseHandlerBuilder builder)
  {
    return builder.AcceptUri(BaseUri).RespondStatusCode(HttpStatusCode.OK);
  }

  private static MessageCaseHandlerBuilder HeadPlaintext(MessageCaseHandlerBuilder builder)
  {
    return builder.AcceptRoute(HttpMethod.Head, PlainTextGetRoute)
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

  public record SumIntsJsonRequest
  {
    [JsonPropertyName("ints")] public int[] Ints { get; set; } = Array.Empty<int>();
  }

  public record SumIntsJsonResponse
  {
    [JsonPropertyName("sum")] public int Sum { get; set; }
  }
}

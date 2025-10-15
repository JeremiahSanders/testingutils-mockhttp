using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Jds.TestingUtils.MockHttp.Tests.Unit.MessageCaseHandlerBuilderExtensionsTests;

public class RespondWithAsyncResponseTests
{
  /// <summary>
  ///   Creates a function which will respond with the provided response as JSON after the provided delay with status 200.
  ///   If the request is canceled, the response will have a 400 status code.
  /// </summary>
  /// <param name="delayInMilliseconds"></param>
  /// <param name="expectedResponse"></param>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  private static Func<HttpResponseMessageBuilder, CapturedHttpRequestMessage, CancellationToken,
      Task<HttpResponseMessageBuilder>>
    CreateResponseFunc<T>(int delayInMilliseconds, T expectedResponse)
  {
    return RespondWith;

    async Task<HttpResponseMessageBuilder> RespondWith(HttpResponseMessageBuilder builder,
      CapturedHttpRequestMessage request, CancellationToken cxlToken)
    {
      if (cxlToken.IsCancellationRequested) // The incoming request was canceled.
      {
        return builder.WithStatusCode(HttpStatusCode.BadRequest);
      }

      try
      {
        // Add an artificial delay to simulate async work.
        // Use of the provided cancellation token allows the wait to abort if the request is canceled.
        // However, we have to catch that exception and return a client-caused error response.
        //   In a real HttpClient request, if your CancellationToken is canceled,
        //   it's actually the HttpClient which throws the TaskCanceledException.
        await Task.Delay(delayInMilliseconds, cxlToken);
      }
      catch (TaskCanceledException)
      {
        return builder.WithStatusCode(HttpStatusCode.BadRequest);
      }

      builder.WithStatusCode(HttpStatusCode.OK)
        .WithContent(JsonContent.Create(expectedResponse));
      return builder;
    }
  }

  [Fact]
  public async Task WhenBuiltIntoMockHttp_GivenAsyncDelay__GivenCancellationOfRequestDuringDelay_RespondsAsExpected()
  {
    var value = Guid.NewGuid().ToString();
    var expectedResponse = new FauxResponse { value = value };

    const int delayInMilliseconds = 30;
    const int cancellationDelayInMilliseconds = delayInMilliseconds / 2;

    var mockHttpBuilder = new MockHttpBuilder()
      .WithHandler("all",
        builder => builder.AcceptAll().RespondWith(CreateResponseFunc(delayInMilliseconds, expectedResponse)));
    using var httpClient = mockHttpBuilder.BuildHttpClient();


    var requestMessage = new HttpRequestMessage(HttpMethod.Patch, "https://not-real");


    CancellationTokenSource cxlSource = new(cancellationDelayInMilliseconds);

    try
    {
      var httpResponse = await httpClient.SendAsync(requestMessage, cxlSource.Token);

      throw new InvalidOperationException("Should not get here. HttpClient should have thrown an exception.");
    }
    catch (TaskCanceledException taskCanceledException)
    {
      taskCanceledException.Source.Should().Be(typeof(HttpClient).Namespace);
      taskCanceledException.StackTrace.Should().Contain(nameof(HttpClient));
      taskCanceledException.CancellationToken.Should().Be(cxlSource.Token);
    }
  }

  [Fact]
  public async Task GivenAsyncDelay_GivenCancellationOfRequestDuringDelay_RespondsAsExpected()
  {
    var value = Guid.NewGuid().ToString();
    var expectedResponse = new FauxResponse { value = value };

    const int delayInMilliseconds = 30;
    const int cancellationDelayInMilliseconds = delayInMilliseconds / 2;

    var caseHandler = new MessageCaseHandlerBuilder()
      .AcceptAll()
      .RespondWith(CreateResponseFunc(delayInMilliseconds, expectedResponse))
      .Build();
    var requestMessage = new HttpRequestMessage(HttpMethod.Patch, "https://not-real");
    var capturedRequest = await CapturedHttpRequestMessage.FromHttpRequestMessage(requestMessage);


    CancellationTokenSource cxlSource = new(cancellationDelayInMilliseconds);

    var response = await caseHandler.HandleMessage(capturedRequest, cxlSource.Token);

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task GivenAsyncDelay_ResponseRespectsDelay()
  {
    var value = Guid.NewGuid().ToString();
    var expectedResponse = new FauxResponse { value = value };

    const int delayInMilliseconds = 15;

    var caseHandler = new MessageCaseHandlerBuilder()
      .AcceptAll()
      .RespondWith(CreateResponseFunc(delayInMilliseconds, expectedResponse))
      .Build();
    var requestMessage = new HttpRequestMessage(HttpMethod.Patch, "https://not-real");
    var capturedRequest = await CapturedHttpRequestMessage.FromHttpRequestMessage(requestMessage);

    var timer = Stopwatch.StartNew();
    var response = await caseHandler.HandleMessage(capturedRequest, CancellationToken.None);
    timer.Stop();

    timer.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(delayInMilliseconds);
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    (await response.Content.ReadFromJsonAsync<FauxResponse>()).Should().BeEquivalentTo(expectedResponse);
  }

  private record FauxResponse
  {
    public string value { get; init; } = string.Empty;
  }
}

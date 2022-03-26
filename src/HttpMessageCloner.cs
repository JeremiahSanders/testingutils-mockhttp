namespace Jds.TestingUtils.MockHttp;

internal static class HttpMessageCloner
{
  public static async Task<(HttpRequestMessage, CapturedHttpRequestMessage)> CloneRequest(
    HttpRequestMessage request,
    CancellationToken cancellationToken
  )
  {
    var capturedRequest = await CapturedHttpRequestMessage.FromHttpRequestMessage(request, cancellationToken);
    var clonedRequest = CapturedHttpRequestMessage.ToHttpRequestMessage(capturedRequest);
    return (clonedRequest, capturedRequest);
  }

  public static async Task<(HttpResponseMessage, CapturedHttpResponseMessage)> CloneResponse(
    HttpResponseMessage response,
    CancellationToken cancellationToken
  )
  {
    var capturedResponse = await CapturedHttpResponseMessage.FromHttpResponseMessage(response, cancellationToken);
    var clonedResponse = CapturedHttpResponseMessage.ToHttpResponseMessage(capturedResponse);
    return (clonedResponse, capturedResponse);
  }
}

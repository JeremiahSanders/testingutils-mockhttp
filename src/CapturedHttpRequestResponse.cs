namespace Jds.TestingUtils.MockHttp;

/// <summary>
///   A captured <see cref="HttpRequestMessage" /> and <see cref="HttpResponseMessage" /> interaction.
/// </summary>
/// <param name="Request">A <see cref="CapturedHttpRequestMessage" />.</param>
/// <param name="Response">A <see cref="CapturedHttpResponseMessage" />.</param>
public record CapturedHttpRequestResponse(CapturedHttpRequestMessage Request, CapturedHttpResponseMessage Response);

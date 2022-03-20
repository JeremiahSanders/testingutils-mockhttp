namespace Jds.TestingUtils.MockHttp.Tests.Unit;

internal static class TestData
{
  public static HttpMethod[] HttpMethods { get; } =
  {
    HttpMethod.Delete, HttpMethod.Get, HttpMethod.Head, HttpMethod.Options, HttpMethod.Patch, HttpMethod.Post,
    HttpMethod.Put, HttpMethod.Trace
  };
}

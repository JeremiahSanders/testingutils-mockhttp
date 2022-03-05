using System.Collections.Specialized;
using System.Web;

namespace Jds.TestingUtils.MockHttp;

/// <summary>
///   Extensions to <see cref="Uri" /> which support matching values in message accept rules.
/// </summary>
public static class UriExtensions
{
  /// <summary>
  ///   Executes <see cref="HttpUtility.ParseQueryString(string)" /> using the <see cref="Uri.Query" /> from
  ///   <paramref name="uri" />.
  /// </summary>
  /// <param name="uri">A <see cref="Uri" />.</param>
  /// <returns>
  ///   A <see cref="NameValueCollection" />. Query parameters which were duplicated, e.g., arrays, have values
  ///   concatenated with commas.
  /// </returns>
  public static NameValueCollection ParseQuery(this Uri uri)
  {
    return HttpUtility.ParseQueryString(uri.Query);
  }

  /// <summary>
  ///   Parses the <see cref="Uri.Query" /> using <see cref="HttpUtility.ParseQueryString(string)" />.
  /// </summary>
  /// <remarks>Use <see cref="GetQueryValues" /> if the query parameter has multiple values.</remarks>
  /// <param name="uri">A <see cref="Uri" />.</param>
  /// <param name="name">A query parameter name.</param>
  /// <returns>A value, if found.</returns>
  public static string? GetQueryValue(this Uri uri, string name)
  {
    return uri.ParseQuery().Get(name);
  }

  /// <summary>
  ///   Parses the <see cref="Uri.Query" /> using <see cref="HttpUtility.ParseQueryString(string)" /> and splits the results
  ///   on commas (,).
  /// </summary>
  /// <param name="uri">A <see cref="Uri" />.</param>
  /// <param name="name">A query parameter name.</param>
  /// <returns>An array of values.</returns>
  public static string[] GetQueryValues(this Uri uri, string name)
  {
    return uri.GetQueryValue(name)?.Split(',') ?? Array.Empty<string>();
  }

  /// <summary>
  ///   Checks whether <paramref name="uri" /> contains a query parameter named <paramref name="name" /> having a value of
  ///   <paramref name="expectedValue" />.
  /// </summary>
  /// <param name="uri">A <see cref="Uri" />.</param>
  /// <param name="name">A query parameter name.</param>
  /// <param name="expectedValue">An expected parameter value to seek.</param>
  /// <returns>A value indicating whether the value is present.</returns>
  public static bool QueryValueContains(this Uri uri, string name, string expectedValue)
  {
    return uri.GetQueryValues(name).Contains(expectedValue);
  }
}

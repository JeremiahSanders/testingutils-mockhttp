using System.Collections.Concurrent;
using System.Xml.Serialization;

namespace Jds.TestingUtils.MockHttp;

/// <summary>
///   <see cref="XmlSerializer" /> cache.
/// </summary>
internal static class XmlSerialization
{
  private static readonly ConcurrentDictionary<Type, WeakReference<XmlSerializer>> GeneratedSerializers = new();

  /// <summary>
  ///   Gets an <see cref="XmlSerializer" /> for <paramref name="type" />.
  /// </summary>
  /// <param name="type">A <see cref="Type" />.</param>
  /// <returns>An <see cref="XmlSerializer" />.</returns>
  /// <remarks>
  ///   Maintains a cache of <see cref="XmlSerializer" /> <see cref="WeakReference{T}" /> to enable caching without
  ///   holding unused instances.
  /// </remarks>
  public static XmlSerializer GetSerializer(Type type)
  {
    XmlSerializer serializer;
    if (GeneratedSerializers.ContainsKey(type) &&
        GeneratedSerializers[type].TryGetTarget(out var cachedSerializer))
    {
      serializer = cachedSerializer;
    }
    else
    {
      serializer = new XmlSerializer(type);
      GeneratedSerializers[type] = new WeakReference<XmlSerializer>(serializer);
    }

    return serializer;
  }
}

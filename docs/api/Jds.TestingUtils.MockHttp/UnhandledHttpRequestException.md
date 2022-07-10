# UnhandledHttpRequestException class

An exception thrown when an [`ArrangedHttpMessageHandler`](./ArrangedHttpMessageHandler.md) receives a HttpRequestMessage which has no arranged handler.

```csharp
public class UnhandledHttpRequestException : Exception
```

## Public Members

| name | description |
| --- | --- |
| [UnhandledHttpRequestException](UnhandledHttpRequestException/UnhandledHttpRequestException.md)(…) | Initializes a new instance of the [`UnhandledHttpRequestException`](./UnhandledHttpRequestException.md) class. (2 constructors) |
| [ContentBytes](UnhandledHttpRequestException/ContentBytes.md) { get; } | Gets the unhandled HttpRequestMessageContent, read as a ByteArray. |
| [ContentString](UnhandledHttpRequestException/ContentString.md) { get; } | Gets the unhandled HttpRequestMessageContent, read as a String. |
| [Headers](UnhandledHttpRequestException/Headers.md) { get; } | Gets the unhandled HttpRequestMessageHeaders. |
| [Method](UnhandledHttpRequestException/Method.md) { get; } | Gets the unhandled HttpRequestMessageMethod. |
| [RequestUri](UnhandledHttpRequestException/RequestUri.md) { get; } | Gets the unhandled HttpRequestMessageRequestUri. |

## See Also

* namespace [Jds.TestingUtils.MockHttp](../TestingUtils.MockHttp.md)
* [UnhandledHttpRequestException.cs](https://github.com/JeremiahSanders/testingutils-mockhttp/src/UnhandledHttpRequestException.cs)

<!-- DO NOT EDIT: generated by xmldocmd for TestingUtils.MockHttp.dll -->
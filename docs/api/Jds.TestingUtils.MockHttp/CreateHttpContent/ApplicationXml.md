# CreateHttpContent.ApplicationXml&lt;T&gt; method

Serializes *contentBody* as XML and returns it as HttpContent.

```csharp
public static HttpContent ApplicationXml<T>(T contentBody, 
    MediaTypeHeaderValue? contentType = null, XmlSerializer? serializer = null)
```

| parameter | description |
| --- | --- |
| T | A serializable content body object type. |
| contentBody | A content body object to serialize. |
| contentType | An optional MediaTypeHeaderValue, e.g., `application/soap+xml`. Defaults to Xml. |
| serializer | An optional XmlSerializer. If not provided, one will be created. |

## Return Value

HttpContent

## Exceptions

| exception | condition |
| --- | --- |
| ArgumentNullException | Thrown when *contentBody* is null. |
| InvalidOperationException | Thrown when serialization fails. |

## See Also

* class [CreateHttpContent](../CreateHttpContent.md)
* namespace [Jds.TestingUtils.MockHttp](../../TestingUtils.MockHttp.md)

<!-- DO NOT EDIT: generated by xmldocmd for TestingUtils.MockHttp.dll -->
# HttpResponseMessageBuilder.WithVersion method

Sets the Version.

```csharp
public HttpResponseMessageBuilder WithVersion(Version version)
```

| parameter | description |
| --- | --- |
| version | An HTTP message version. Per Version documentation, the default is `1.1`. |

## Return Value

This instance.

## Remarks

This method does not set an application/API version or headers. It sets the Version. Overriding the default value, `1.1`, is uncommon.

## See Also

* class [HttpResponseMessageBuilder](../HttpResponseMessageBuilder.md)
* namespace [Jds.TestingUtils.MockHttp](../../TestingUtils.MockHttp.md)

<!-- DO NOT EDIT: generated by xmldocmd for TestingUtils.MockHttp.dll -->
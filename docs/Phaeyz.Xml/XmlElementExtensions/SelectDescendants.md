# XmlElementExtensions.SelectDescendants method

Selects descendant elements of the current XmlElement.

```csharp
public static IEnumerable<XmlElement> SelectDescendants(this XmlElement @this, 
    string? namespaceUri = null, string? localName = null)
```

| parameter | description |
| --- | --- |
| this | The XmlElement to select descendant elements. |
| namespaceUri | Optionally filters all elements to the specified namespace URI. Specify `null` for all namespaces. The default is `null` if unspecified. |
| localName | Optionally filters all elements to the specified local name. Specify `null` for all local names. The default is `null` if unspecified. |

## Return Value

An enumerable of matching elements.

## See Also

* class [XmlElementExtensions](../XmlElementExtensions.md)
* namespace [Phaeyz.Xml](../../Phaeyz.Xml.md)

<!-- DO NOT EDIT: generated by xmldocmd for Phaeyz.Xml.dll -->

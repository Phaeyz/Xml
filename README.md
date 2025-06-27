# Phaeyz

Phaeyz is a set of libraries created and polished over time for use with other projects, and made available here for convenience.

All Phaeyz libraries may be found [here](https://github.com/Phaeyz).

# Phaeyz.Xml

API documentation for **Phaeyz.Xml** library is [here](https://github.com/Phaeyz/Xml/blob/main/docs/Phaeyz.Xml.md).

This library contains a set of XML utilities and XML extension methods which are useful for other projects, such as [Phaeyz.Jfif](https://github.com/Phaeyz/Jfif). Here are some highlights.

## XmlPrefixedNamespace

```C#
XmlPrefixedNamespace namedPrefix = new("prefix1", "ns://foo"); // Prefixed namespace are now instances
XmlPrefixedNamespace defaultPrefix = new(null, "ns://bar"); // Default prefix works too
bool r = xmlAttribute.GetPrefixedNamespace() == namedPrefix; // Instances may be compared
xmlParentElement.AddNamespaceDeclaration(namedPrefix); // They can be added as declarations on elements
XmlAttribute attr = xmlParentElement.AddAttribute(namedPrefix, "attr", "value"); // They can be used when creating attributes
XmlElement el = xmlParentElement.AddChildElement(namedPrefix, "el"); // They can be used when creating elements
```

## XmlDocument.OptimizeNamespaces()

```C#
// Efficiently optimizes namespaces by removing redundant and unnecessary declarations.
// Namespaces will not be moved, and prefixes will be preserved and honored.
xmlDocument.OptimizeNamespaces();
```

## XmlDocument.Render()

```C#
// Produces the same output as XDocument.ToString().
string xml = xmlDocument.Render();
```

# Licensing

This project is licensed under GNU General Public License v3.0, which means you can use it for personal or educational purposes for free. However, donations are always encouraged to support the ongoing development of adding new features and resolving issues.

If you plan to use this code for commercial purposes or within an organization, we kindly ask for a donation to support the project's development. Any reasonably sized donation amount which reflects the perceived value of using Phaeyz in your product or service is accepted.

## Donation Options

There are several ways to support Phaeyz with a donation. Perhaps the best way is to use GitHub Sponsors or Patreon so that recurring small donations continue to support the development of Phaeyz.

- **GitHub Sponsors**: [https://github.com/sponsors/phaeyz](https://github.com/sponsors/phaeyz)
- **Patreon**: [https://www.patreon.com/phaeyz](https://www.patreon.com/phaeyz)
- **Bitcoin**: Send funds to address: ```bc1qdzdahz8d7jkje09fg7s7e8xedjsxm6kfhvsgsw```
- **PayPal**: Send funds to ```phaeyz@pm.me``` ([directions](https://www.paypal.com/us/cshelp/article/how-do-i-send-money-help293))

Your support is greatly appreciated and helps me continue to improve and maintain Phaeyz!
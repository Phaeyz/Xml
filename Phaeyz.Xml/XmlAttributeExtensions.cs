using System.Xml;

namespace Phaeyz.Xml;

/// <summary>
/// Extension methods for <see cref="System.Xml.XmlAttribute"/>.
/// </summary>
public static class XmlAttributeExtensions
{
    /// <summary>
    /// Gets the <see cref="Phaeyz.Xml.XmlPrefixedNamespace"/> for the attribute.
    /// </summary>
    /// <param name="this">
    /// The <see cref="System.Xml.XmlAttribute"/> to get the <see cref="Phaeyz.Xml.XmlPrefixedNamespace"/> for.
    /// </param>
    /// <returns>
    /// Returns the <see cref="Phaeyz.Xml.XmlPrefixedNamespace"/> for the attribute.
    /// </returns>
    public static XmlPrefixedNamespace GetPrefixedNamespace(this XmlAttribute @this)
    {
        ArgumentNullException.ThrowIfNull(@this);
        return @this.NamespaceURI == CommonNamespaces.Xmlns
            ? new XmlPrefixedNamespace(@this.Prefix == "xmlns" ? @this.LocalName : string.Empty, @this.Value)
            : new XmlPrefixedNamespace(@this.Prefix, @this.NamespaceURI);
    }

    /// <summary>
    /// Determines if the attribute is a namespace declaration.
    /// </summary>
    /// <param name="this">
    /// The <see cref="System.Xml.XmlAttribute"/> to determine if it is a namespace declaration.
    /// </param>
    /// <returns>
    /// Returns <c>true</c> if the attribute is a namespace declaration; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNamespaceDeclaration(this XmlAttribute @this) => @this?.NamespaceURI == CommonNamespaces.Xmlns;
}
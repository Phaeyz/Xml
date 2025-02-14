using System.Reflection;
using System.Text;
using System.Xml;

namespace Phaeyz.Xml;

/// <summary>
/// Extension methods for <see cref="System.Xml.XmlElement"/>.
/// </summary>
public static class XmlElementExtensions
{
    /// <summary>
    /// Internally used to set the <c>XmlName</c> property on an element.
    /// </summary>
    private static readonly Lazy<PropertyInfo> XmlNamePropertyInfo = new(() =>
        typeof(XmlElement).GetProperty("XmlName", BindingFlags.NonPublic | BindingFlags.Instance)!);

    /// <summary>
    /// Adds a namespace declaration to the element. The element is not checked for existing declarations.
    /// </summary>
    /// <param name="this">
    /// The <see cref="System.Xml.XmlElement"/> to add the namespace declaration to.
    /// </param>
    /// <param name="prefixedNamespace">
    /// The namespace declaration to add.
    /// </param>
    /// <returns>
    /// The <see cref="System.Xml.XmlElement"/> passed in as input.
    /// </returns>
    public static XmlElement AddNamespaceDeclaration(this XmlElement @this, XmlPrefixedNamespace prefixedNamespace)
    {
        ArgumentNullException.ThrowIfNull(@this);
        ArgumentNullException.ThrowIfNull(prefixedNamespace);

        XmlAttribute attr = prefixedNamespace.Prefix.Length == 0
            ? @this.OwnerDocument.CreateAttribute(null, "xmlns", CommonNamespaces.Xmlns)
            : @this.OwnerDocument.CreateAttribute("xmlns", prefixedNamespace.Prefix, CommonNamespaces.Xmlns);
        attr.Value = prefixedNamespace.Uri;

        @this.Attributes.Append(attr);
        return @this;
    }

    /// <summary>
    /// Adds an attribute to the element.
    /// </summary>
    /// <param name="this">
    /// The <see cref="System.Xml.XmlElement"/> to add the attribute to.
    /// </param>
    /// <param name="prefixedNamespace">
    /// The namespace of the attribute to add.
    /// </param>
    /// <param name="localName">
    /// The local name of the attribute.
    /// </param>
    /// <param name="value">
    /// The value of the attribute.
    /// </param>
    /// <returns>
    /// The created attribute.
    /// </returns>
    public static XmlAttribute AddAttribute(this XmlElement @this, XmlPrefixedNamespace prefixedNamespace, string localName, string value)
    {
        ArgumentNullException.ThrowIfNull(@this);
        ArgumentNullException.ThrowIfNull(prefixedNamespace);
        ArgumentNullException.ThrowIfNullOrEmpty(localName);
        ArgumentNullException.ThrowIfNull(value);
        XmlAttribute attr = @this.OwnerDocument.CreateAttribute(prefixedNamespace.Prefix, localName, prefixedNamespace.Uri);
        attr.Value = value;
        @this.Attributes.Append(attr);
        return attr;
    }

    /// <summary>
    /// Adds a child element to the element.
    /// </summary>
    /// <param name="this">
    /// The <see cref="System.Xml.XmlElement"/> to add the child element to.
    /// </param>
    /// <param name="prefixedNamespace">
    /// The namespace of the child element to add.
    /// </param>
    /// <param name="localName"></param>
    /// <returns>
    /// The created child element.
    /// </returns>
    public static XmlElement AddChildElement(this XmlElement @this, XmlPrefixedNamespace prefixedNamespace, string localName)
    {
        ArgumentNullException.ThrowIfNull(@this);
        ArgumentNullException.ThrowIfNull(prefixedNamespace);
        ArgumentNullException.ThrowIfNullOrEmpty(localName);
        XmlElement element = @this.OwnerDocument.CreateElement(prefixedNamespace.Prefix, localName, prefixedNamespace.Uri);
        @this.AppendChild(element);
        return element;
    }

    /// <summary>
    /// Ensures there is a namespace declaration for the given namespace name and prefix.
    /// </summary>
    /// <param name="this">
    /// The <see cref="System.Xml.XmlElement"/> to ensure there is a namespace declaration.
    /// </param>
    /// <param name="declareEvenIfInherited">
    /// If <c>true</c>, a namespace declaration will be added even if the namespace is inherited from a parent element.
    /// If <c>false</c>, a namespace declaration will not be added if the namespace is inherited from a parent element.
    /// </param>
    /// <param name="prefixedNamespace">
    /// The namespace to ensure is declared.
    /// </param>
    /// <returns>
    /// The <see cref="System.Xml.XmlElement"/> passed in as input.
    /// </returns>
    public static XmlElement EnsureNamespaceDeclared(this XmlElement @this, bool declareEvenIfInherited, XmlPrefixedNamespace prefixedNamespace)
    {
        ArgumentNullException.ThrowIfNull(@this);
        ArgumentNullException.ThrowIfNull(prefixedNamespace);

        // First see if a namespace declaration with this prefix already exists. If so, remove it if it is
        // not the same namespace as the one we want to add.
        bool alreadyDeclared = false;
        foreach (XmlAttribute attr in @this.Attributes.Cast<XmlAttribute>().Where(attr => attr.IsNamespaceDeclaration()).ToList())
        {
            string attrPrefix = attr.LocalName;
            if (attrPrefix is null || attrPrefix == "xmlns")
            {
                attrPrefix = string.Empty;
            }

            if (prefixedNamespace.Prefix == attrPrefix)
            {
                if (alreadyDeclared || attr.Value != prefixedNamespace.Uri)
                {
                    @this.Attributes.Remove(attr);
                }
                else
                {
                    alreadyDeclared = true;
                }
            }
        }

        // If we didn't find a declaration, add one, but only if it is not already inherited or we
        // are disregarding inheritance. Don't call GetNamespaceOfPrefix() on the current element because
        // it may not return an accurate value (see the work-around below).
        if (!alreadyDeclared &&
            (declareEvenIfInherited || @this.ParentNode is null ||
            @this.ParentNode.GetNamespaceOfPrefix(prefixedNamespace.Prefix) != prefixedNamespace.Uri))
        {
            @this.AddNamespaceDeclaration(prefixedNamespace);
        }

        // Work-around a .NET limitation (or bug?) where the namespace URI of the element is not updated.
        if (@this.Prefix == prefixedNamespace.Prefix && @this.NamespaceURI != prefixedNamespace.Uri)
        {
            @this.SetElementNamespaceUri(prefixedNamespace.Uri);
        }

        return @this;
    }

    /// <summary>
    /// Gets a set of all namespaces which are referenced but not declared in the element or its descendants.
    /// </summary>
    /// <param name="this">
    /// The element to scan for undeclared namespaces.
    /// </param>
    /// <returns>
    /// A set of all namespaces which are referenced but not declared when referenced.
    /// </returns>
    /// <remarks>
    /// This does not perform validation of the XML document such as redefinition of namespaces using the same prefix,
    /// or elements which use a prefix but the namespace URI is wrong. A namespace is considered declared as long as a
    /// prefix is defined (including the default prefix).
    /// </remarks>
    public static HashSet<XmlPrefixedNamespace> GetNamespacesNotDeclaredInProgeny(this XmlElement @this)
    {
        ArgumentNullException.ThrowIfNull(@this);
        HashSet<string> prefixes = [];
        HashSet<XmlPrefixedNamespace> undeclaredNamespaces = [];
        @this.GetNamespacesNotDeclaredInProgenyRecursively(prefixes, undeclaredNamespaces);
        return undeclaredNamespaces;
    }

    /// <summary>
    /// Recursively scans the element and its descendants for namespaces which are referenced but not declared.
    /// </summary>
    /// <param name="this">
    /// The element to scan for undeclared namespaces.
    /// </param>
    /// <param name="prefixesDefinedInAncestry">
    /// Prefixes known to be defined in the ancestry to this point in the hierarchy.
    /// </param>
    /// <param name="undeclaredNamespaces">
    /// The set of namespaces which have been referenced but not declared when referenced.
    /// </param>
    private static void GetNamespacesNotDeclaredInProgenyRecursively(
        this XmlElement @this,
        HashSet<string> prefixesDefinedInAncestry,
        HashSet<XmlPrefixedNamespace> undeclaredNamespaces)
    {
        HashSet<string> prefixes = new(prefixesDefinedInAncestry);
        // Collect all the declared prefixes this element.
        foreach (XmlAttribute attr in @this.SelectAttributes().Where(attr => attr.IsNamespaceDeclaration()))
        {
            string attrPrefix = attr.LocalName;
            if (attrPrefix is null || attrPrefix == "xmlns")
            {
                attrPrefix = string.Empty;
            }
            prefixes.Add(attrPrefix);
        }
        // Check for namespace dependencies of all other attributes
        foreach (XmlAttribute attr in @this.SelectAttributes().Where(attr => !attr.IsNamespaceDeclaration()))
        {
            // Ignore the default namespace.
            if ((attr.Prefix.Length > 0 || attr.NamespaceURI.Length > 0) && !prefixes.Contains(attr.Prefix))
            {
                undeclaredNamespaces.Add(new(attr.Prefix, attr.NamespaceURI));
            }
        }
        // Check for namespace dependencies of all the current element. Ignore the default namespace.
        if ((@this.Prefix.Length > 0 || @this.NamespaceURI.Length > 0) && !prefixes.Contains(@this.Prefix))
        {
            undeclaredNamespaces.Add(new(@this.Prefix, @this.NamespaceURI));
        }
        // Do the same check for each child.
        foreach (XmlElement child in @this.SelectChildren())
        {
            child.GetNamespacesNotDeclaredInProgenyRecursively(prefixes, undeclaredNamespaces);
        }
    }

    /// <summary>
    /// Gets the <see cref="XmlPrefixedNamespace"/> for the element.
    /// </summary>
    /// <param name="this">
    /// The <see cref="System.Xml.XmlElement"/> to get the <see cref="Phaeyz.Xml.XmlPrefixedNamespace"/> for.
    /// </param>
    /// <returns>
    /// Returns the <see cref="XmlPrefixedNamespace"/> for the element.
    /// </returns>
    public static XmlPrefixedNamespace GetPrefixedNamespace(this XmlElement @this)
    {
        ArgumentNullException.ThrowIfNull(@this);
        return new XmlPrefixedNamespace(@this.Prefix, @this.NamespaceURI);
    }

    /// <summary>
    /// Gets the parent element of the current element.
    /// </summary>
    /// <param name="this">
    /// The element for which to get the parent element.
    /// </param>
    /// <returns>
    /// The parent element of the current element.
    /// </returns>
    /// <remarks>
    /// This is basically a strongly-typed version of <c>ParentNode</c>.
    /// </remarks>
    public static XmlElement? GetParentElement(this XmlElement @this)
    {
        ArgumentNullException.ThrowIfNull(@this);
        return @this.ParentNode as XmlElement;
    }

    /// <summary>
    /// Selects attributes of the current <see cref="System.Xml.XmlElement"/>.
    /// </summary>
    /// <param name="this">
    /// The <see cref="System.Xml.XmlElement"/> to select attributes from.
    /// </param>
    /// <param name="namespaceUri">
    /// Optionally filters all attributes to the specified namespace URI. Specify <c>null</c> for all namespaces.
    /// The default is <c>null</c> if unspecified.
    /// </param>
    /// <param name="localName">
    /// Optionally filters all attributes to the specified local name. Specify <c>null</c> for all local names.
    /// The default is <c>null</c> if unspecified.
    /// </param>
    /// <returns>
    /// An enumerable of matching attributes.
    /// </returns>
    public static IEnumerable<XmlAttribute> SelectAttributes(this XmlElement @this, string? namespaceUri = null, string? localName = null)
    {
        ArgumentNullException.ThrowIfNull(@this);
        if (localName == string.Empty)
        {
            throw new ArgumentException("The local name cannot be an empty string.", nameof(localName));
        }
        return @this
            .Attributes
            .OfType<XmlAttribute>()
            .Where(attr => namespaceUri is null || attr.NamespaceURI == namespaceUri)
            .Where(attr => localName is null || attr.LocalName == localName);
    }

    /// <summary>
    /// Selects child elements of the current <see cref="System.Xml.XmlElement"/>.
    /// </summary>
    /// <param name="this">
    /// The <see cref="System.Xml.XmlElement"/> to select child elements from.
    /// </param>
    /// <param name="namespaceUri">
    /// Optionally filters all child elements to the specified namespace URI. Specify <c>null</c> for all namespaces.
    /// The default is <c>null</c> if unspecified.
    /// </param>
    /// <param name="localName">
    /// Optionally filters all child elements to the specified local name. Specify <c>null</c> for all local names.
    /// The default is <c>null</c> if unspecified.
    /// </param>
    /// <returns>
    /// An enumerable of matching child elements.
    /// </returns>
    public static IEnumerable<XmlElement> SelectChildren(
        this XmlElement @this,
        string? namespaceUri = null,
        string? localName = null)
    {
        ArgumentNullException.ThrowIfNull(@this);
        if (localName == string.Empty)
        {
            throw new ArgumentException("The local name cannot be an empty string.", nameof(localName));
        }
        return @this
            .ChildNodes
            .OfType<XmlElement>()
            .Where(child => namespaceUri is null || child.NamespaceURI == namespaceUri)
            .Where(child => localName is null || child.LocalName == localName);
    }

    /// <summary>
    /// Selects descendant elements of the current <see cref="System.Xml.XmlElement"/>.
    /// </summary>
    /// <param name="this">
    /// The <see cref="System.Xml.XmlElement"/> to select descendant elements.
    /// </param>
    /// <param name="namespaceUri">
    /// Optionally filters all elements to the specified namespace URI. Specify <c>null</c> for all namespaces.
    /// The default is <c>null</c> if unspecified.
    /// </param>
    /// <param name="localName">
    /// Optionally filters all elements to the specified local name. Specify <c>null</c> for all local names.
    /// The default is <c>null</c> if unspecified.
    /// </param>
    /// <returns>
    /// An enumerable of matching elements.
    /// </returns>
    public static IEnumerable<XmlElement> SelectDescendants(
        this XmlElement @this,
        string? namespaceUri = null,
        string? localName = null)
    {
        ArgumentNullException.ThrowIfNull(@this);
        if (localName == string.Empty)
        {
            throw new ArgumentException("The local name cannot be an empty string.", nameof(localName));
        }
        foreach (XmlElement child in @this.SelectChildren(namespaceUri, localName))
        {
            yield return child;
            foreach (XmlElement descendant in child.SelectDescendants(namespaceUri, localName))
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Selects descendant elements and the current <see cref="System.Xml.XmlElement"/>.
    /// </summary>
    /// <param name="this">
    /// The <see cref="System.Xml.XmlElement"/> to select along with descendant elements.
    /// </param>
    /// <param name="namespaceUri">
    /// Optionally filters all elements to the specified namespace URI. Specify <c>null</c> for all namespaces.
    /// The default is <c>null</c> if unspecified.
    /// </param>
    /// <param name="localName">
    /// Optionally filters all elements to the specified local name. Specify <c>null</c> for all local names.
    /// The default is <c>null</c> if unspecified.
    /// </param>
    /// <returns>
    /// An enumerable of matching elements.
    /// </returns>
    public static IEnumerable<XmlElement> SelectDescendantsAndSelf(
        this XmlElement @this,
        string? namespaceUri = null,
        string? localName = null)
    {
        ArgumentNullException.ThrowIfNull(@this);
        if (localName == string.Empty)
        {
            throw new ArgumentException("The local name cannot be an empty string.", nameof(localName));
        }
        if ((namespaceUri is null || @this.NamespaceURI == namespaceUri) ||
            (localName is null || @this.LocalName == localName))
        {
            yield return @this;
        }
        foreach (XmlElement descendant in @this.SelectDescendants(namespaceUri, localName))
        {
            yield return descendant;
        }
    }

    /// <summary>
    /// When changing attributes, the owning element's namespace URI is not updated
    /// if the attributes would enforce it. This bug is documented here: https://github.com/dotnet/runtime/issues/111738
    /// </summary>
    /// <param name="this">
    /// The <see cref="System.Xml.XmlElement"/> to update the namespace URI of.
    /// </param>
    /// <param name="namespaceUri">
    /// The new namespace URI to set.
    /// </param>
    private static void SetElementNamespaceUri(this XmlElement @this, string namespaceUri)
    {
        ArgumentNullException.ThrowIfNull(@this);
        XmlNamePropertyInfo.Value.SetValue(
            @this,
            XmlNamePropertyInfo.Value.GetValue(@this.OwnerDocument.CreateElement(@this.Prefix, @this.LocalName, namespaceUri)));
    }
}

using System.Text;
using System.Xml;

namespace Phaeyz.Xml;

/// <summary>
/// Extension methods for <see cref="System.Xml.XmlDocument"/>.
/// </summary>
public static class XmlDocumentExtensions
{
    /// <summary>
    /// UTF-16 encoding without a byte-order mark. Used for Render(). System.Text.Encoding.Unicode
    /// enforces byte-order marks.
    /// </summary>
    private static readonly Encoding s_utf16WithoutBom = new UnicodeEncoding(false, false);

    /// <summary>
    /// Adds a element to the document.
    /// </summary>
    /// <param name="this">
    /// The <see cref="System.Xml.XmlDocument"/> to add the element to.
    /// </param>
    /// <param name="prefixedNamespace">
    /// The prefixed namespace to use for the new element. Validation is not performed to ensure the prefix
    /// and namespace is not already defined to the namespace.
    /// </param>
    /// <param name="localName"></param>
    /// <returns>
    /// The created element.
    /// </returns>
    public static XmlElement AddElement(this XmlDocument @this, XmlPrefixedNamespace prefixedNamespace, string localName)
    {
        ArgumentNullException.ThrowIfNull(@this);
        ArgumentNullException.ThrowIfNull(prefixedNamespace);
        ArgumentNullException.ThrowIfNullOrEmpty(localName);
        XmlElement element = @this.CreateElement(prefixedNamespace.Prefix, localName, prefixedNamespace.Uri);
        @this.AppendChild(element);
        return element;
    }

    /// <summary>
    /// Efficiently optimizes namespaces by removing redundant and unnecessary declarations.
    /// </summary>
    /// <param name="this">
    /// The <see cref="System.Xml.XmlDocument"/> to optimize namespaces for.
    /// </param>
    /// <returns>
    /// The input <see cref="System.Xml.XmlDocument"/> instance.
    /// </returns>
    /// <remarks>
    /// Namespaces will not be moved, and prefixes will be preserved and honored.
    /// </remarks>
    public static XmlDocument OptimizeNamespaces(this XmlDocument @this) => NamespaceOptimizer.Optimize(@this);

    /// <summary>
    /// Renders the XML document to a string.
    /// </summary>
    /// <param name="this">
    /// The <see cref="System.Xml.XmlDocument"/> to render as a string.
    /// </param>
    /// <returns>
    /// The XML document rendered as a string.
    /// </returns>
    /// <remarks>
    /// The resulting XML string will omit the XML declaration and be indented with two spaces.
    /// This is effectively the same as <c>XDocument.ToString()</c>.
    /// </remarks>
    public static string Render(this XmlDocument @this)
    {
        XmlWriterSettings settings = new()
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = s_utf16WithoutBom,
            OmitXmlDeclaration = true,
        };

        var sb = new StringBuilder();
        using (XmlWriter xmlWriter = XmlWriter.Create(sb, settings))
        {
            @this.Save(xmlWriter);
        }
        return sb.ToString();
    }
}

/// <summary>
/// Internally performs the logic for optimizing namespaces.
/// </summary>
file class NamespaceOptimizer
{
    private readonly XmlElement _element;
    private readonly Dictionary<string, NamespaceDeclaration> _declarations;
    private readonly HashSet<NamespaceDeclaration> _declarationsThisElement = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Phaeyz.Xml.NamespaceOptimizer"/> class for the given element.
    /// </summary>
    /// <param name="element">
    /// The current element being optimized.
    /// </param>
    /// <param name="copyFrom">
    /// Optionally, an instance to inherit (copy) the namespace information from.
    /// </param>
    private NamespaceOptimizer(XmlElement element, NamespaceOptimizer? copyFrom = null)
    {
        _element = element;
        _declarations = copyFrom is null ? [] : new(copyFrom._declarations);
    }

    /// <summary>
    /// Gets the namespace declaration for the given namespace name, if it exists.
    /// </summary>
    /// <param name="prefix">
    /// The prefix of the namespace.
    /// </param>
    /// <returns>
    /// The namespace declaration.
    /// </returns>
    private NamespaceDeclaration? GetNamespaceDeclaration(string prefix) =>
        _declarations.TryGetValue(prefix, out NamespaceDeclaration? namespaceDeclaration)
            ? namespaceDeclaration
            : null;

    /// <summary>
    /// The main entry point for optimizing the namespaces of the given document.
    /// </summary>
    /// <param name="document">
    /// The XML document for which to optimize namespaces.
    /// </param>
    /// <returns>
    /// The XML document which was passed in as input.
    /// </returns>
    public static XmlDocument Optimize(XmlDocument document)
    {
        if (document.DocumentElement is not null)
        {
            new NamespaceOptimizer(document.DocumentElement).RecursivelyOptimize();
        }

        return document;
    }

    /// <summary>
    /// Recursively optimizes the namespaces of the current element and its children.
    /// </summary>
    private void RecursivelyOptimize()
    {
        // Track all namespace declarations. While tracking it, if the declaration is
        // redundant, instead remove it from the element.
        foreach (XmlAttribute attr in _element.Attributes.Cast<XmlAttribute>().Where(attr => attr.IsNamespaceDeclaration()).ToList())
        {
            if (!TrackNamespaceDeclaration(attr))
            {
                _element.Attributes.Remove(attr);
            }
        }

        // Reference the namespace prefix for the element.
        GetNamespaceDeclaration(_element.Prefix)?.AddReference();

        // Reference the namespace prefix for each of the attributes.
        foreach (XmlAttribute attr in _element.Attributes.Cast<XmlAttribute>().Where(attr => !attr.IsNamespaceDeclaration()).ToList())
        {
            GetNamespaceDeclaration(attr.Prefix)?.AddReference();
        }

        // Recursively optimize the children. This may also add references to namespaces
        // tracked for this element.
        foreach (XmlElement child in _element.ChildNodes.OfType<XmlElement>())
        {
            new NamespaceOptimizer(child, this).RecursivelyOptimize();
        }

        // Finally, remove all namespace declaration attributes which have not been referenced.
        foreach (NamespaceDeclaration? declaration in _declarationsThisElement.Where(d => d.ReferenceCount == 0))
        {
            _element.Attributes.Remove(declaration.Attribute);
        }
    }

    /// <summary>
    /// Tracks a namespace declaration.
    /// </summary>
    /// <param name="attr">
    /// The attribute of the namespace declaration to track.
    /// </param>
    /// <returns>
    /// Returns <c>true</c> if the namespace declaration is not redundant and therefore tracked.
    /// Returns <c>false</c> if the namespace declaration is redundant and therefore not tracked.
    /// </returns>
    private bool TrackNamespaceDeclaration(XmlAttribute attr)
    {
        // If the local name is null, empty, or "xmlns", it is the default namespace. Normalize to a non-null value.
        string prefix = attr.LocalName;
        if (prefix is null || prefix == "xmlns")
        {
            prefix = string.Empty;
        }

        // If the namespace declaration is not tracked, and it is a default namespace declaration,
        // this is the default scenario and an unnecessary declaration. On the other hand,
        // if there is already a tracked namespace declaration which matches the namespace URI,
        // it is redundant. Also, if the elements match it means the current element has more than
        // one namespace declaration for the same prefix, which is technically invalid XML.
        NamespaceDeclaration? namespaceDeclaration = GetNamespaceDeclaration(prefix);
        if ((namespaceDeclaration is null && prefix.Length == 0 && attr.Value.Length == 0) ||
            (namespaceDeclaration is not null &&
                (namespaceDeclaration.Element == _element || namespaceDeclaration.Attribute.Value == attr.Value)))
        {
            return false;
        }

        // Track the namespace declaration.
        namespaceDeclaration = new() { Element = _element, Attribute = attr, Prefix = prefix };
        _declarationsThisElement.Add(namespaceDeclaration);
        _declarations[prefix] = namespaceDeclaration;
        return true;
    }

    /// <summary>
    /// Used to track a namespace declaration and it's reference count.
    /// </summary>
    private class NamespaceDeclaration
    {
        public required XmlElement Element;
        public required XmlAttribute Attribute;
        public required string Prefix;
        public int ReferenceCount;

        public void AddReference() => ReferenceCount++;
    }
}

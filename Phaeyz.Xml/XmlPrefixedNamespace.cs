namespace Phaeyz.Xml;

/// <summary>
/// Represents a namespace with an optional prefix.
/// </summary>
public class XmlPrefixedNamespace : IEquatable<XmlPrefixedNamespace>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Phaeyz.Xml.XmlPrefixedNamespace"/> class.
    /// </summary>
    /// <param name="prefix">
    /// The prefix of the namespace. If the prefix is null, empty, or "xmlns",
    /// it is normalized to the default namespace (an empty string).
    /// </param>
    /// <param name="namespaceUri">
    /// The namespace URI.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    /// Namespace URI cannot be empty if a prefix is provided.
    /// </exception>
    public XmlPrefixedNamespace(string? prefix, string namespaceUri)
    {
        Prefix = NormalizePrefix(prefix);
        Uri = namespaceUri ?? string.Empty;

        if (Uri.Length == 0 && Prefix.Length > 0)
        {
            throw new ArgumentNullException(nameof(namespaceUri), "Namespace URI cannot be empty if a prefix is provided.");
        }
    }

    /// <summary>
    /// The prefix of the namespace.
    /// </summary>
    public string Prefix { get; private init; }

    /// <summary>
    /// The namespace URI.
    /// </summary>
    public string Uri { get; private init; }

    /// <summary>
    /// Deconstructs the <see cref="Phaeyz.Xml.XmlPrefixedNamespace"/> into its components.
    /// </summary>
    /// <param name="prefix">
    /// The namespace prefix.
    /// </param>
    /// <param name="namespaceUri">
    /// The namespace URI.
    /// </param>
    public void Deconstruct(out string prefix, out string namespaceUri)
    {
        prefix = Prefix;
        namespaceUri = Uri;
    }

    /// <summary>
    /// Tests if the current instance is equal to another instance.
    /// </summary>
    /// <param name="other">
    /// The other instance to compare to.
    /// </param>
    /// <returns>
    /// Returns <c>true</c> if the current instance is equal to the other instance; <c>false</c> otherwise.
    /// </returns>
    public bool Equals(XmlPrefixedNamespace? other) => Prefix == other?.Prefix && Uri == other?.Uri;

    /// <summary>
    /// Determines whether two object instances are equal.
    /// </summary>
    /// <param name="obj">
    /// The object to compare with the current object.
    /// </param>
    /// <returns>
    /// <c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object? obj) => Equals(obj as XmlPrefixedNamespace);

    /// <summary>
    /// Computes a hash for the current instance.
    /// </summary>
    /// <returns>
    /// A hash for the current instance.
    /// </returns>
    public override int GetHashCode() => HashCode.Combine(Prefix, Uri);

    /// <summary>
    /// If the prefix is null, empty, or "xmlns", it is the default namespace. Normalize to a non-null value.
    /// </summary>
    /// <param name="prefix">
    /// The prefix to normalize.
    /// </param>
    /// <returns>
    /// The normalized prefix value.
    /// </returns>
    public static string NormalizePrefix(string? prefix) => prefix is null || prefix == "xmlns" ? string.Empty : prefix;

    /// <summary>
    /// Creates a friendly string for the current instance.
    /// </summary>
    /// <returns>
    /// A friendly string for the current instance.
    /// </returns>
    public override string ToString() => Prefix.Length == 0 ? $"xmlns=\"{Uri}\"" : $"xmlns:{Prefix}=\"{Uri}\"";

    /// <summary>
    /// Tests if two instances are equal.
    /// </summary>
    /// <param name="a">
    /// The first instance.
    /// </param>
    /// <param name="b">
    /// The second instance.
    /// </param>
    /// <returns>
    /// Returns <c>true</c> if the two instances are equal; <c>false</c> otherwise.
    /// </returns>
    public static bool operator ==(XmlPrefixedNamespace? a, XmlPrefixedNamespace? b) => a is null ? b is null : a.Equals(b);

    /// <summary>
    /// Tests if two instances are not equal.
    /// </summary>
    /// <param name="a">
    /// The first instance.
    /// </param>
    /// <param name="b">
    /// The second instance.
    /// </param>
    /// <returns>
    /// Returns <c>true</c> if the two instances are not equal; <c>false</c> otherwise.
    /// </returns>
    public static bool operator !=(XmlPrefixedNamespace? a, XmlPrefixedNamespace? b) => a is null ? b is not null : !a.Equals(b);

    /// <summary>
    /// Implicitly converts a namespace URI to an <see cref="Phaeyz.Xml.XmlPrefixedNamespace"/> with an empty prefix.
    /// </summary>
    /// <param name="namespaceUri">
    /// The namespace URI.
    /// </param>
    public static implicit operator XmlPrefixedNamespace(string namespaceUri) => new(null, namespaceUri);

    /// <summary>
    /// Implicitly converts the <see cref="Phaeyz.Xml.XmlPrefixedNamespace"/> to a tuple.
    /// </summary>
    /// <param name="xmlPrefixedNamespace">
    /// The <see cref="Phaeyz.Xml.XmlPrefixedNamespace"/> to convert to a tuple.
    /// </param>
    public static implicit operator (string prefix, string namespaceUri)(XmlPrefixedNamespace xmlPrefixedNamespace) =>
        (xmlPrefixedNamespace.Prefix, xmlPrefixedNamespace.Uri);

    /// <summary>
    /// Implicitly converts a tuple to an <see cref="Phaeyz.Xml.XmlPrefixedNamespace"/>.
    /// </summary>
    /// <param name="xmlPrefixedNamespace">
    /// The tuple to convert to an <see cref="Phaeyz.Xml.XmlPrefixedNamespace"/>.
    /// </param>
    public static implicit operator XmlPrefixedNamespace((string prefix, string namespaceUri) xmlPrefixedNamespace) =>
        new(xmlPrefixedNamespace.prefix, xmlPrefixedNamespace.namespaceUri);
}
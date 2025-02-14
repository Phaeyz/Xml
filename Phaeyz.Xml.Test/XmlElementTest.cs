using System.Xml;

namespace Phaeyz.Xml.Test;

internal class XmlElementExtensionsTest
{
    private static async Task AssertAttributes(XmlAttributeCollection actual, IEnumerable<(string ln, string ns)> expected)
    {
        List<(string ln, string xmlns, string ns)> actualList = actual
            .Cast<XmlAttribute>()
            .Select(attr => (ln: attr.LocalName, xmlns: attr.NamespaceURI, ns: attr.Value))
            .Order()
            .ToList();

        List<(string ln, string xmlns, string ns)> expectedList = expected
            .Select(o => (o.ln, xmlns: CommonNamespaces.Xmlns, o.ns))
            .Order()
            .ToList();

        await Assert.That(() => actualList).IsEquivalentTo(expectedList);
    }

    private static XmlElement RunEnsureNamespaceDeclaredTest(
        string inputXml,
        bool declareEvenIfInherited,
        string namespaceUri,
        string? prefix = null,
        bool useFirstChild = true)
    {
        XmlDocument document = new();
        document.LoadXml(inputXml);
        XmlElement el = useFirstChild
            ? (XmlElement)document.DocumentElement!.FirstChild!
            : document.DocumentElement!;
        return el.EnsureNamespaceDeclared(declareEvenIfInherited, new(prefix, namespaceUri));
    }

    private static async Task RunGetNamespacesNotDeclaredInProgenyTest(
        string inputXml,
        HashSet<XmlPrefixedNamespace> expectedNamespaces)
    {
        XmlDocument document = new();
        document.LoadXml(inputXml);
        XmlElement el = (XmlElement)document.DocumentElement!.FirstChild!;
        HashSet<XmlPrefixedNamespace> actualNamespaces = el.GetNamespacesNotDeclaredInProgeny();
        await Assert.That(() => actualNamespaces.Count).IsEqualTo(expectedNamespaces.Count);
        foreach (XmlPrefixedNamespace actualNamespace in actualNamespaces)
        {
            await Assert.That(() => expectedNamespaces.Contains(actualNamespace)).IsTrue();
        }
    }

    #region EnsureNamespaceDeclared_ForceDeclareFalseParentNoneChildPrefixed

        [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseParentNoneChildPrefixed_ChildHasNewDeclaration()
    {
        const string input = """
            <root>
              <child></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, false, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("prefix", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseParentNoneChildPrefixedOneExisting_ChildHasDeclaration()
    {
        const string input = """
            <root>
              <child xmlns:prefix="ns"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, false, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("prefix", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseParentNoneChildPrefixedDupNamespace_ChildHasDeclaration()
    {
        const string input = """
            <root>
              <child xmlns:dup1="ns" xmlns:dup2="ns"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, false, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("prefix", "ns"),
            ("dup1", "ns"),
            ("dup2", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseParentNoneChildPrefixedPrefixHasDifferentNamespace_ChildRedeclaration()
    {
        const string input = """
            <root>
              <child xmlns:prefix="wrong"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, false, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("prefix", "ns")
            ]);
    }

    #endregion EnsureNamespaceDeclared_ForceDeclareFalseParentNoneChildPrefixed

    #region EnsureNamespaceDeclared_ForceDeclareTrueParentNoneChildPrefixed

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueParentNoneChildPrefixed_ChildHasNewDeclaration()
    {
        const string input = """
            <root>
              <child></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, true, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("prefix", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueParentNoneChildPrefixedOneExisting_ChildHasDeclaration()
    {
        const string input = """
            <root>
              <child xmlns:prefix="ns"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, true, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("prefix", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueParentNoneChildPrefixedDupNamespace_ChildHasDeclaration()
    {
        const string input = """
            <root>
              <child xmlns:dup1="ns" xmlns:dup2="ns"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, true, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("prefix", "ns"),
            ("dup1", "ns"),
            ("dup2", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueParentNoneChildPrefixedPrefixHasDifferentNamespace_ChildRedeclaration()
    {
        const string input = """
            <root>
              <child xmlns:prefix="wrong"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, true, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("prefix", "ns")
            ]);
    }

    #endregion EnsureNamespaceDeclared_ForceDeclareTrueParentNoneChildPrefixed

    #region EnsureNamespaceDeclared_ForceDeclareFalseParentPrefixedChildPrefixed

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseParentPrefixedChildPrefixed_ChildDoesNotDeclare()
    {
        const string input = """
            <root xmlns:prefix="ns">
              <child></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, false, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, []);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseParentPrefixedChildPrefixedOneExisting_ChildHasDeclaration()
    {
        const string input = """
            <root xmlns:prefix="ns">
              <child xmlns:prefix="ns"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, false, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("prefix", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseParentPrefixedChildPrefixedDupNamespace_ChildHasDeclaration()
    {
        const string input = """
            <root xmlns:prefix="ns">
              <child xmlns:dup1="ns" xmlns:dup2="ns"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, false, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("dup1", "ns"),
            ("dup2", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseParentPrefixedChildPrefixedPrefixHasDifferentNamespace_WrongDeclarationRemovedFromChild()
    {
        const string input = """
            <root xmlns:prefix="ns">
              <child xmlns:prefix="wrong"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, false, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, []);
    }

    #endregion EnsureNamespaceDeclared_ForceDeclareFalseParentPrefixedChildPrefixed

    #region EnsureNamespaceDeclared_ForceDeclareTrueParentPrefixedChildPrefixed

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueParentPrefixedChildPrefixed_ChildHasNewDeclaration()
    {
        const string input = """
            <root xmlns:prefix="ns">
              <child></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, true, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("prefix", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueParentPrefixedChildPrefixedOneExisting_ChildHasDeclaration()
    {
        const string input = """
            <root xmlns:prefix="ns">
              <child xmlns:prefix="ns"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, true, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("prefix", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueParentPrefixedChildPrefixedDupNamespace_ChildHasDeclaration()
    {
        const string input = """
            <root xmlns:prefix="ns">
              <child xmlns:dup1="ns" xmlns:dup2="ns"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, true, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("prefix", "ns"),
            ("dup1", "ns"),
            ("dup2", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueParentPrefixedChildPrefixedPrefixHasDifferentNamespace_ChildRedeclaration()
    {
        const string input = """
            <root xmlns:prefix="ns">
              <child xmlns:prefix="wrong"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, true, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("prefix", "ns")
            ]);
    }

    #endregion EnsureNamespaceDeclared_ForceDeclareTrueParentPrefixedChildPrefixed

    #region EnsureNamespaceDeclared_ForceDeclareFalseParentUnprefixedChildPrefixed

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseParentUnprefixedChildPrefixed_ChildHasNewDeclaration()
    {
        const string input = """
            <root xmlns="ns">
              <child></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, false, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("prefix", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseParentUnprefixedChildPrefixedOneExisting_ChildHasDeclaration()
    {
        const string input = """
            <root xmlns="ns">
              <child xmlns="ns"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, false, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("xmlns", "ns"),
            ("prefix", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseParentUnprefixedChildPrefixedDupNamespace_ChildHasDeclaration()
    {
        const string input = """
            <root xmlns="ns">
              <child xmlns:dup1="ns" xmlns:dup2="ns"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, false, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("prefix", "ns"),
            ("dup1", "ns"),
            ("dup2", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseParentUnprefixedChildPrefixedPrefixHasDifferentNamespace_ChildRedeclaration()
    {
        const string input = """
            <root xmlns="ns">
              <child xmlns:prefix="wrong"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, false, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("prefix", "ns")
            ]);
    }

    #endregion EnsureNamespaceDeclared_ForceDeclareFalseParentUnprefixedChildPrefixed

    #region EnsureNamespaceDeclared_ForceDeclareTrueParentUnprefixedChildPrefixed

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueParentUnprefixedChildPrefixed_ChildHasNewDeclaration()
    {
        const string input = """
            <root xmlns="ns">
              <child></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, true, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("prefix", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueParentUnprefixedChildPrefixedOneExisting_ChildHasDeclaration()
    {
        const string input = """
            <root xmlns="ns">
              <child xmlns:prefix="ns"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, true, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("prefix", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueParentUnprefixedChildPrefixedDupNamespace_ChildHasDeclaration()
    {
        const string input = """
            <root xmlns="ns">
              <child xmlns:dup1="ns" xmlns:dup2="ns"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, true, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("prefix", "ns"),
            ("dup1", "ns"),
            ("dup2", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueParentUnprefixedChildPrefixedPrefixHasDifferentNamespace_ChildRedeclaration()
    {
        const string input = """
            <root xmlns="ns">
              <child xmlns:prefix="wrong"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, true, "ns", "prefix");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("prefix", "ns")
            ]);
    }

    #endregion EnsureNamespaceDeclared_ForceDeclareTrueParentUnprefixedChildPrefixed

    #region EnsureNamespaceDeclared_ForceDeclareFalseParentNoneChildUnprefixed

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseParentNoneChildUnprefixed_ChildHasNewDeclaration()
    {
        const string input = """
            <root>
              <child></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, false, "ns");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("xmlns", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseParentNoneChildUnprefixedOneExisting_ChildHasDeclaration()
    {
        const string input = """
            <root>
              <child xmlns="ns"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, false, "ns");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("xmlns", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseParentNoneChildUnprefixedDupNamespace_ChildHasDeclaration()
    {
        const string input = """
            <root>
              <child xmlns:dup1="ns" xmlns:dup2="ns"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, false, "ns");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("xmlns", "ns"),
            ("dup1", "ns"),
            ("dup2", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseParentNoneChildUnprefixedPrefixHasDifferentNamespace_ChildRedeclaration()
    {
        const string input = """
            <root>
              <child xmlns="wrong"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, false, "ns");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("xmlns", "ns")
            ]);
    }

    #endregion EnsureNamespaceDeclared_ForceDeclareFalseParentNoneChildUnprefixed

    #region EnsureNamespaceDeclared_ForceDeclareTrueParentNoneChildUnprefixed

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueParentNoneChildUnprefixed_ChildHasNewDeclaration()
    {
        const string input = """
            <root>
              <child></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, true, "ns");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("xmlns", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueParentNoneChildUnprefixedOneExisting_ChildHasDeclaration()
    {
        const string input = """
            <root>
              <child xmlns="ns"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, true, "ns");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("xmlns", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueParentNoneChildUnprefixedDupNamespace_ChildHasDeclaration()
    {
        const string input = """
            <root>
              <child xmlns:dup1="ns" xmlns:dup2="ns"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, true, "ns");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("xmlns", "ns"),
            ("dup1", "ns"),
            ("dup2", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueParentNoneChildUnprefixedPrefixHasDifferentNamespace_ChildRedeclaration()
    {
        const string input = """
            <root>
              <child xmlns="wrong"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, true, "ns");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("xmlns", "ns")
            ]);
    }

    #endregion EnsureNamespaceDeclared_ForceDeclareTrueParentNoneChildUnprefixed

    #region EnsureNamespaceDeclared_ForceDeclareFalseParentUnprefixedChildUnprefixed

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseParentUnprefixedChildUnprefixed_NewAttributeNotDeclared()
    {
        const string input = """
            <root xmlns="ns">
              <child></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, false, "ns");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseParentUnprefixedChildUnprefixedOneExisting_ChildHasDeclaration()
    {
        const string input = """
            <root xmlns="ns">
              <child xmlns="ns"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, false, "ns");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("xmlns", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseParentUnprefixedChildUnprefixedDupNamespace_NewAttributeNotDeclared()
    {
        const string input = """
            <root xmlns="ns">
              <child xmlns:dup1="ns" xmlns:dup2="ns"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, false, "ns");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("dup1", "ns"),
            ("dup2", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseParentUnprefixedChildUnprefixedPrefixHasDifferentNamespace_WrongAttributeRemoved()
    {
        const string input = """
            <root xmlns="ns">
              <child xmlns="wrong"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, false, "ns");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ]);
    }

    #endregion EnsureNamespaceDeclared_ForceDeclareFalseParentUnprefixedChildUnprefixed

    #region EnsureNamespaceDeclared_ForceDeclareTrueParentUnprefixedChildUnprefixed

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueParentUnprefixedChildUnprefixed_ChildHasNewDeclaration()
    {
        const string input = """
            <root xmlns="ns">
              <child></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, true, "ns");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("xmlns", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueParentUnprefixedChildUnprefixedOneExisting_ChildHasDeclaration()
    {
        const string input = """
            <root xmlns="ns">
              <child xmlns="ns"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, true, "ns");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("xmlns", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueParentUnprefixedChildUnprefixedDupNamespace_ChildHasDeclaration()
    {
        const string input = """
            <root xmlns="ns">
              <child xmlns:dup1="ns" xmlns:dup2="ns"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, true, "ns");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("xmlns", "ns"),
            ("dup1", "ns"),
            ("dup2", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueParentUnprefixedChildUnprefixedPrefixHasDifferentNamespace_ChildRedeclaration()
    {
        const string input = """
            <root xmlns="ns">
              <child xmlns="wrong"></child>
            </root>
            """;
        XmlElement child = RunEnsureNamespaceDeclaredTest(input, true, "ns");
        await Assert.That(() => child.LocalName).IsEqualTo("child");
        await AssertAttributes(child.Attributes, [
            ("xmlns", "ns")
            ]);
    }

    #endregion EnsureNamespaceDeclared_ForceDeclareTrueParentUnprefixedChildUnprefixed

    #region Force Declare False Root Tests

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseRootPrefixedWithWrongNamespace_NamespaceChanged()
    {
        const string input = """
            <prefix:root xmlns:prefix="wrong"></prefix:root>
            """;
        XmlElement el = RunEnsureNamespaceDeclaredTest(input, false, "ns", "prefix", false);
        await Assert.That(() => el.NamespaceURI).IsEqualTo("ns");
        await AssertAttributes(el.Attributes, [
            ("prefix", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseRootPrefixedWithCorrectNamespace_NamespaceUnchanged()
    {
        const string input = """
            <prefix:root xmlns:prefix="ns"></prefix:root>
            """;
        XmlElement el = RunEnsureNamespaceDeclaredTest(input, false, "ns", "prefix", false);
        await Assert.That(() => el.NamespaceURI).IsEqualTo("ns");
        await AssertAttributes(el.Attributes, [
            ("prefix", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseRootUnprefixedWithWrongNamespace_NamespaceChanged()
    {
        const string input = """
            <root xmlns="wrong"></root>
            """;
        XmlElement el = RunEnsureNamespaceDeclaredTest(input, false, "ns", null, false);
        await Assert.That(() => el.NamespaceURI).IsEqualTo("ns");
        await AssertAttributes(el.Attributes, [
            ("xmlns", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseRootUnprefixedWithCorrectNamespace_NamespaceUnchanged()
    {
        const string input = """
            <root xmlns="ns"></root>
            """;
        XmlElement el = RunEnsureNamespaceDeclaredTest(input, false, "ns", null, false);
        await Assert.That(() => el.NamespaceURI).IsEqualTo("ns");
        await AssertAttributes(el.Attributes, [
            ("xmlns", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareFalseRootNone_NamespaceAdded()
    {
        const string input = """
            <root></root>
            """;
        XmlElement el = RunEnsureNamespaceDeclaredTest(input, false, "ns", null, false);
        await Assert.That(() => el.NamespaceURI).IsEqualTo("ns");
        await AssertAttributes(el.Attributes, [
            ("xmlns", "ns")
            ]);
    }

    #endregion Force Declare False Root Tests

    #region Force Declare True Root Tests

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueRootPrefixedWithWrongNamespace_NamespaceChanged()
    {
        const string input = """
            <prefix:root xmlns:prefix="wrong"></prefix:root>
            """;
        XmlElement el = RunEnsureNamespaceDeclaredTest(input, true, "ns", "prefix", false);
        await Assert.That(() => el.NamespaceURI).IsEqualTo("ns");
        await AssertAttributes(el.Attributes, [
            ("prefix", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueRootPrefixedWithCorrectNamespace_NamespaceUnchanged()
    {
        const string input = """
            <prefix:root xmlns:prefix="ns"></prefix:root>
            """;
        XmlElement el = RunEnsureNamespaceDeclaredTest(input, true, "ns", "prefix", false);
        await Assert.That(() => el.NamespaceURI).IsEqualTo("ns");
        await AssertAttributes(el.Attributes, [
            ("prefix", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueRootUnprefixedWithWrongNamespace_NamespaceChanged()
    {
        const string input = """
            <root xmlns="wrong"></root>
            """;
        XmlElement el = RunEnsureNamespaceDeclaredTest(input, true, "ns", null, false);
        await Assert.That(() => el.NamespaceURI).IsEqualTo("ns");
        await AssertAttributes(el.Attributes, [
            ("xmlns", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueRootUnprefixedWithCorrectNamespace_NamespaceUnchanged()
    {
        const string input = """
            <root xmlns="ns"></root>
            """;
        XmlElement el = RunEnsureNamespaceDeclaredTest(input, true, "ns", null, false);
        await Assert.That(() => el.NamespaceURI).IsEqualTo("ns");
        await AssertAttributes(el.Attributes, [
            ("xmlns", "ns")
            ]);
    }

    [Test]
    public async Task EnsureNamespaceDeclared_ForceDeclareTrueRootNone_NamespaceAdded()
    {
        const string input = """
            <root></root>
            """;
        XmlElement el = RunEnsureNamespaceDeclaredTest(input, true, "ns", null, false);
        await Assert.That(() => el.NamespaceURI).IsEqualTo("ns");
        await AssertAttributes(el.Attributes, [
            ("xmlns", "ns")
            ]);
    }

    #endregion Force Declare True Root Tests

    #region GetNamespacesNotDeclaredInProgeny

    [Test]
    public async Task GetNamespacesNotDeclaredInProgeny_ComplexGraph_ExpectedNamespacesReturned()
    {
        const string xml = """
            <root xmlns="ns1" xmlns:test2="ns2" xmlns:test5="ns5" xmlns:test6="unused6">
              <test2:child>
                <grandchild1 xmlns:test3="ns3" test3:foo="bar"></grandchild1>
                <test2:grandchild2 xmlns="ns4" xmlns:test7="unused7">
                  <descendant test5:abc="123"></descendant>
                </test2:grandchild2>
              </test2:child>
            </root>
            """;
        await RunGetNamespacesNotDeclaredInProgenyTest(
            xml,
            [new(string.Empty, "ns1"), new("test2", "ns2"), new("test5", "ns5")]);
    }

    [Test]
    public async Task GetNamespacesNotDeclaredInProgeny_DefaultAndEmptyNamespace_NothingReturned()
    {
        const string xml = """
            <root>
              <child></child>
            </root>
            """;
        await RunGetNamespacesNotDeclaredInProgenyTest(xml, []);
    }

    [Test]
    public async Task GetNamespacesNotDeclaredInProgeny_DefaultNonEmptyNamespace_DefaultNamespaceReturned()
    {
        const string xml = """
            <root xmlns="ns">
              <child></child>
            </root>
            """;
        await RunGetNamespacesNotDeclaredInProgenyTest(xml, [new(string.Empty, "ns")]);
    }

    #endregion GetNamespacesNotDeclaredInProgeny
}

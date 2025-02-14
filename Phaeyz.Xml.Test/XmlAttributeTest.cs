using System.Xml;

namespace Phaeyz.Xml.Test;

public class XmlAttributeTest
{
    private static async Task RunGetPrefixedNamespaceTest(
        string xml,
        XmlPrefixedNamespace expected)
    {
        XmlDocument xmlDoc = new();
        xmlDoc.LoadXml(xml);
        XmlAttribute? attr = xmlDoc.DocumentElement!.Attributes.OfType<XmlAttribute>().FirstOrDefault();
        if (attr is null)
        {
            Assert.Fail("No attributes found.");
        }
        else
        {
            await Assert.That(() => attr.GetPrefixedNamespace()).IsEqualTo(expected);
        }
    }

    [Test]
    public async Task GetPrefixedNamespace_DefaultNamespaceIsDefault_UnprefixedEmptyNamespace()
    {
        const string xml = """
            <root xmlns=""></root>
            """;
        await RunGetPrefixedNamespaceTest(xml, new(string.Empty, string.Empty));
    }

    [Test]
    public async Task GetPrefixedNamespace_DefaultNamespaceIsTest_UnprefixedNsNamespace()
    {
        const string xml = """
            <root xmlns="ns"></root>
            """;
        await RunGetPrefixedNamespaceTest(xml, new(string.Empty, "ns"));
    }

    [Test]
    public async Task GetPrefixedNamespace_PrefixNamespaceIsDefault_PrefixedNsNamespace()
    {
        const string xml = """
            <root xmlns:test="ns"></root>
            """;
        await RunGetPrefixedNamespaceTest(xml, new("test", "ns"));
    }
}

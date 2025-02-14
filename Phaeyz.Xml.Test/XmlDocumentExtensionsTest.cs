using System.Xml;

namespace Phaeyz.Xml.Test;

internal class XmlDocumentExtensionsTest
{
    private static string ParseOptimizeAndRender(string inputXml)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(inputXml);
        xmlDocument.OptimizeNamespaces();
        return xmlDocument.Render();
    }

    [Test]
    public async Task OptimizeNamespaces_ComplexGraph_MatchesExpected()
    {
        const string input = """
            <root xmlns="ns:default1/">
              <test1:test xmlns:test1="ns:test1/">
                <foo test1:attr="bar" xmlns="ns:default2/">
                  <abc xmlns:test1="ns:test1-override/" test1:override="test"></abc>
                </foo>
              </test1:test>
              <test1:test xmlns:test1="ns:test1/" xmlns:unused="ns:remove/"></test1:test>
              <test>
                <test1:bar xmlns:test1="ns:test1-override/"></test1:bar>
              </test>
            </root>
            """;
        const string expected = """
            <root xmlns="ns:default1/">
              <test1:test xmlns:test1="ns:test1/">
                <foo test1:attr="bar" xmlns="ns:default2/">
                  <abc xmlns:test1="ns:test1-override/" test1:override="test"></abc>
                </foo>
              </test1:test>
              <test1:test xmlns:test1="ns:test1/"></test1:test>
              <test>
                <test1:bar xmlns:test1="ns:test1-override/"></test1:bar>
              </test>
            </root>
            """;

        await Assert.That(() => ParseOptimizeAndRender(input)).IsEqualTo(expected);
    }

    [Test]
    public async Task OptimizeNamespaces_EmptyDefaultNotWrittenAtRoot_MatchesExpected()
    {
        const string input = """
            <root xmlns="">
              <a xmlns="">
                <b xmlns="ns:test1/">
                  <c xmlns="ns:test1/">
                    <d xmlns="">
                      <e xmlns="">
                        <f xmlns="ns:test2/">
                        </f>
                      </e>
                    </d>
                  </c>
                </b>
              </a>
            </root>
            """;
        const string expected = """
            <root>
              <a>
                <b xmlns="ns:test1/">
                  <c>
                    <d xmlns="">
                      <e>
                        <f xmlns="ns:test2/"></f>
                      </e>
                    </d>
                  </c>
                </b>
              </a>
            </root>
            """;
        await Assert.That(() => ParseOptimizeAndRender(input)).IsEqualTo(expected);
    }

    [Test]
    public async Task OptimizeNamespaces_NoNamespaces_MatchesExpected()
    {
        const string input = """
            <root>
              <a>
                <b>
                  <c>
                    <d>
                      <e>
                        <f>
                          <g>
                            <h>
                            </h>
                          </g>
                        </f>
                      </e>
                    </d>
                  </c>
                </b>
              </a>
            </root>
            """;
        const string expected = """
            <root>
              <a>
                <b>
                  <c>
                    <d>
                      <e>
                        <f>
                          <g>
                            <h></h>
                          </g>
                        </f>
                      </e>
                    </d>
                  </c>
                </b>
              </a>
            </root>
            """;
        await Assert.That(() => ParseOptimizeAndRender(input)).IsEqualTo(expected);
    }

    [Test]
    public async Task OptimizeNamespaces_OneBranchIsDefault_MatchesExpected()
    {
        const string input = """
            <root xmlns="ns:default-override/">
              <a xmlns="ns:default-override/">
              </a>
              <test:b xmlns="" xmlns:test="ns:test/">
                <c test:foo="bar" xmlns="" xmlns:test="ns:test/"></c>
              </test:b>
            </root>
            """;
        const string expected = """
            <root xmlns="ns:default-override/">
              <a></a>
              <test:b xmlns="" xmlns:test="ns:test/">
                <c test:foo="bar"></c>
              </test:b>
            </root>
            """;
        await Assert.That(() => ParseOptimizeAndRender(input)).IsEqualTo(expected);
    }

    [Test]
    public async Task OptimizeNamespaces_OnlyUnused_MatchesExpected()
    {
        const string input = """
            <root xmlns:test0="ns:test0/">
              <a xmlns:test1="ns:test1/">
                <b xmlns:test2="ns:test2/"></b>
                <c xmlns:test2="ns:test2/"></c>
              </a>
              <a xmlns:test1="ns:test1/">
                <b xmlns:test3="ns:test3/"></b>
                <c xmlns:test3="ns:test3/"></c>
              </a>
            </root>
            """;
        const string expected = """
            <root>
              <a>
                <b></b>
                <c></c>
              </a>
              <a>
                <b></b>
                <c></c>
              </a>
            </root>
            """;
        await Assert.That(() => ParseOptimizeAndRender(input)).IsEqualTo(expected);
    }

    [Test]
    public async Task OptimizeNamespaces_PeersWithSamePrefixButDifferentNamespace_MatchesExpected()
    {
        const string input = """
            <root>
              <test:a xmlns:test="ns:test1"></test:a>
              <test:b xmlns:test="ns:test1"></test:b>
              <test:c xmlns:test="ns:test2"></test:c>
              <test:d xmlns:test="ns:test2"></test:d>
            </root>
            """;
        const string expected = """
            <root>
              <test:a xmlns:test="ns:test1"></test:a>
              <test:b xmlns:test="ns:test1"></test:b>
              <test:c xmlns:test="ns:test2"></test:c>
              <test:d xmlns:test="ns:test2"></test:d>
            </root>
            """;
        await Assert.That(() => ParseOptimizeAndRender(input)).IsEqualTo(expected);
    }

    [Test]
    public async Task OptimizeNamespaces_RepeatedDefaultChange_MatchesExpected()
    {
        const string input = """
            <root xmlns="ns:test1/">
              <a xmlns="ns:test1/">
                <b xmlns="ns:test2/">
                  <c xmlns="ns:test2/">
                    <d xmlns="ns:test3/">
                      <e xmlns="ns:test3/">
                        <f xmlns="ns:test1/">
                          <g xmlns="ns:test1/">
                            <h xmlns="ns:test4/">
                            </h>
                          </g>
                        </f>
                      </e>
                    </d>
                  </c>
                </b>
              </a>
            </root>
            """;
        const string expected = """
            <root xmlns="ns:test1/">
              <a>
                <b xmlns="ns:test2/">
                  <c>
                    <d xmlns="ns:test3/">
                      <e>
                        <f xmlns="ns:test1/">
                          <g>
                            <h xmlns="ns:test4/"></h>
                          </g>
                        </f>
                      </e>
                    </d>
                  </c>
                </b>
              </a>
            </root>
            """;
        await Assert.That(() => ParseOptimizeAndRender(input)).IsEqualTo(expected);
    }

    [Test]
    public async Task OptimizeNamespaces_RepeatedReuseOfNamespace_MatchesExpected()
    {
        const string input = """
            <test1:root xmlns:test1="ns:test/">
              <test1:a xmlns:test1="ns:test/">
                <test2:b xmlns:test2="ns:test/">
                  <test2:c xmlns:test2="ns:test/">
                    <test3:d xmlns:test3="ns:test/">
                      <test3:e xmlns:test3="ns:test/">
                        <test1:f xmlns:test1="ns:test/">
                          <test1:g xmlns:test1="ns:test/">
                            <test4:h xmlns:test4="ns:test/">
                            </test4:h>
                          </test1:g>
                        </test1:f>
                      </test3:e>
                    </test3:d>
                  </test2:c>
                </test2:b>
              </test1:a>
            </test1:root>
            """;
        const string expected = """
            <test1:root xmlns:test1="ns:test/">
              <test1:a>
                <test2:b xmlns:test2="ns:test/">
                  <test2:c>
                    <test3:d xmlns:test3="ns:test/">
                      <test3:e>
                        <test1:f>
                          <test1:g>
                            <test4:h xmlns:test4="ns:test/"></test4:h>
                          </test1:g>
                        </test1:f>
                      </test3:e>
                    </test3:d>
                  </test2:c>
                </test2:b>
              </test1:a>
            </test1:root>
            """;
        await Assert.That(() => ParseOptimizeAndRender(input)).IsEqualTo(expected);
    }

    [Test]
    public async Task OptimizeNamespaces_RepeatedReuseOfNamespaceWithoutRedeclaring_PrefixesArePerserved()
    {
        const string input = """
            <test1:root xmlns:test1="ns:test/">
              <test2:a xmlns:test2="ns:test/">
                <test3:b xmlns:test3="ns:test/">
                  <test1:c>
                    <test2:d>
                      <test3:e>
                      </test3:e>
                    </test2:d>
                  </test1:c>
                </test3:b>
              </test2:a>
            </test1:root>
            """;
        const string expected = """
            <test1:root xmlns:test1="ns:test/">
              <test2:a xmlns:test2="ns:test/">
                <test3:b xmlns:test3="ns:test/">
                  <test1:c>
                    <test2:d>
                      <test3:e></test3:e>
                    </test2:d>
                  </test1:c>
                </test3:b>
              </test2:a>
            </test1:root>
            """;
        await Assert.That(() => ParseOptimizeAndRender(input)).IsEqualTo(expected);
    }

    [Test]
    public async Task OptimizeNamespaces_RepeatedReuseOfPrefix_MatchesExpected()
    {
        const string input = """
            <test:root xmlns:test="ns:test1/">
              <test:a xmlns:test="ns:test1/">
                <test:b xmlns:test="ns:test2/">
                  <test:c xmlns:test="ns:test2/">
                    <test:d xmlns:test="ns:test3/">
                      <test:e xmlns:test="ns:test3/">
                        <test:f xmlns:test="ns:test1/">
                          <test:g xmlns:test="ns:test1/">
                            <test:h xmlns:test="ns:test4/">
                            </test:h>
                          </test:g>
                        </test:f>
                      </test:e>
                    </test:d>
                  </test:c>
                </test:b>
              </test:a>
            </test:root>
            """;
        const string expected = """
            <test:root xmlns:test="ns:test1/">
              <test:a>
                <test:b xmlns:test="ns:test2/">
                  <test:c>
                    <test:d xmlns:test="ns:test3/">
                      <test:e>
                        <test:f xmlns:test="ns:test1/">
                          <test:g>
                            <test:h xmlns:test="ns:test4/"></test:h>
                          </test:g>
                        </test:f>
                      </test:e>
                    </test:d>
                  </test:c>
                </test:b>
              </test:a>
            </test:root>
            """;
        await Assert.That(() => ParseOptimizeAndRender(input)).IsEqualTo(expected);
    }

    [Test]
    public async Task OptimizeNamespaces_OneElementTwoPrefixesOneNamespace_PrefixesArePreserved()
    {
        const string input = """
            <root>
              <test1:a xmlns:test1="ns:test">
                <test2:a xmlns:test2="ns:test" xmlns:test3="ns:test" test3:attr="foo"></test2:a>
              </test1:a>
            </root>
            """;
        const string expected = """
            <root>
              <test1:a xmlns:test1="ns:test">
                <test2:a xmlns:test2="ns:test" xmlns:test3="ns:test" test3:attr="foo"></test2:a>
              </test1:a>
            </root>
            """;
        await Assert.That(() => ParseOptimizeAndRender(input)).IsEqualTo(expected);
    }
}

using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using TecnoFisc.Sped.NFeNFCe.Tests.Streaming;

namespace TecnoFisc.Sped.NFeNFCe.Tests;

public sealed class CanonicalSchemaTests
{
    [Fact]
    public async Task CancellationRequestMatchesPinnedEventSchemaAndReadsAsNonEffective()
    {
        const string key = "31210711222333000181550050007156531436322400";
        var request = XDocument.Parse($"""
            <evento versao="1.00" xmlns="http://www.portalfiscal.inf.br/nfe">
              <infEvento Id="ID110111{key}01"><cOrgao>31</cOrgao><tpAmb>1</tpAmb><CNPJ>11222333000181</CNPJ><chNFe>{key}</chNFe><dhEvento>2026-10-02T12:00:00-03:00</dhEvento><tpEvento>110111</tpEvento><nSeqEvento>1</nSeqEvento><verEvento>1.00</verEvento><detEvento versao="1.00"><descEvento>Cancelamento</descEvento><nProt>131214250732778</nProt><xJust>Justificativa de teste</xJust></detEvento></infEvento>
              <Signature xmlns="http://www.w3.org/2000/09/xmldsig#"><SignedInfo><CanonicalizationMethod Algorithm="http://www.w3.org/TR/2001/REC-xml-c14n-20010315"/><SignatureMethod Algorithm="http://www.w3.org/2000/09/xmldsig#rsa-sha1"/><Reference URI="#ID110111{key}01"><Transforms><Transform Algorithm="http://www.w3.org/2000/09/xmldsig#enveloped-signature"/><Transform Algorithm="http://www.w3.org/TR/2001/REC-xml-c14n-20010315"/></Transforms><DigestMethod Algorithm="http://www.w3.org/2000/09/xmldsig#sha1"/><DigestValue>AA==</DigestValue></Reference></SignedInfo><SignatureValue>AA==</SignatureValue><KeyInfo><X509Data><X509Certificate>AA==</X509Certificate></X509Data></KeyInfo></Signature>
            </evento>
            """);
        var schemas = new XmlSchemaSet { XmlResolver = new XmlUrlResolver() };
        schemas.Add("http://www.portalfiscal.inf.br/nfe", Path.Combine(AppContext.BaseDirectory, "Schema", "schemas-eventos", "eventoCancNFe_v1.00.xsd"));
        var errors = new List<string>();
        request.Validate(schemas, (_, e) => errors.Add(e.Message));
        errors.Should().BeEmpty();
        using var input = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(request.ToString()));
        var result = (EventoCancelamento)await new ParserNFe().ReadEventoAsync(input, TestContext.Current.CancellationToken);
        result.IsEffective.Should().BeFalse();
        result.NProt.Should().Be("131214250732778");
    }

    [Fact]
    public void CanonicalFixtureHasValidFiscalStructure()
    {
        XNamespace ns = "http://www.portalfiscal.inf.br/nfe";
        var document = XDocument.Parse(ReadLimitsTests.Fixture);
        var note = document.Descendants(ns + "NFe").Single();
        // Structural signature only; the library never verifies cryptography.
        if (!note.Elements().Any(x => x.Name.LocalName == "Signature"))
            note.Add(XElement.Parse("""
                <Signature xmlns="http://www.w3.org/2000/09/xmldsig#"><SignedInfo><CanonicalizationMethod Algorithm="http://www.w3.org/TR/2001/REC-xml-c14n-20010315"/><SignatureMethod Algorithm="http://www.w3.org/2000/09/xmldsig#rsa-sha1"/><Reference URI="#NFe31210711222333000181550050007156531436322400"><Transforms><Transform Algorithm="http://www.w3.org/2000/09/xmldsig#enveloped-signature"/><Transform Algorithm="http://www.w3.org/TR/2001/REC-xml-c14n-20010315"/></Transforms><DigestMethod Algorithm="http://www.w3.org/2000/09/xmldsig#sha1"/><DigestValue>AA==</DigestValue></Reference></SignedInfo><SignatureValue>AA==</SignatureValue><KeyInfo><X509Data><X509Certificate>AA==</X509Certificate></X509Data></KeyInfo></Signature>
                """));
        var schemas = new XmlSchemaSet { XmlResolver = new XmlUrlResolver() };
        schemas.Add(ns.NamespaceName, Path.Combine(AppContext.BaseDirectory, "Schema", "schemas", "nfe_v4.00.xsd"));
        var errors = new List<string>();
        new XDocument(note).Validate(schemas, (_, e) => errors.Add(e.Message));
        errors.Should().BeEmpty();
    }
}

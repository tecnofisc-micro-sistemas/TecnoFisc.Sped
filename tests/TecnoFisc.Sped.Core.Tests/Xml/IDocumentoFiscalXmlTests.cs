using TecnoFisc.Sped.Xml.Engine;

namespace TecnoFisc.Sped.Core.Tests.Xml;

public class IDocumentoFiscalXmlTests
{
    private const string ChaveValida = "35240111222333000181550010000000011000000018";

    private sealed record DocumentoFake(ChaveAcesso ChaveAcesso) : IDocumentoFiscalXml;

    [Fact]
    public void Documento_Exposes_ChaveAcesso_Through_Contract()
    {
        IDocumentoFiscalXml documento = new DocumentoFake(ChaveAcesso.Create(ChaveValida));

        documento.ChaveAcesso.ToString().Should().Be(ChaveValida);
    }
}

using System.Text;
using TecnoFisc.Sped.Core.Xml;

namespace TecnoFisc.Sped.Core.Tests.Xml;

public sealed class IdentificadorXmlFiscalTests
{
    private const string Ns = "http://www.portalfiscal.inf.br/nfe";

    private static MemoryStream Xml(string conteudo) => new MemoryStream(Encoding.UTF8.GetBytes(conteudo));

    [Fact]
    public async Task Identificar_NFeProcMod55_RetornaNFeProc()
    {
        await using var s = Xml(
            $"<nfeProc versao=\"4.00\" xmlns=\"{Ns}\"><NFe><infNFe><ide><mod>55</mod></ide></infNFe></NFe></nfeProc>");

        var tipo = await IdentificadorXmlFiscal.IdentificarAsync(s, TestContext.Current.CancellationToken);

        tipo.Should().Be(TipoDocumentoFiscalXml.NFeProc);
    }

    [Fact]
    public async Task Identificar_NFeProcMod65_RetornaNFCeProc()
    {
        await using var s = Xml(
            $"<nfeProc versao=\"4.00\" xmlns=\"{Ns}\"><NFe><infNFe><ide><mod>65</mod></ide></infNFe></NFe></nfeProc>");

        var tipo = await IdentificadorXmlFiscal.IdentificarAsync(s, TestContext.Current.CancellationToken);

        tipo.Should().Be(TipoDocumentoFiscalXml.NFCeProc);
    }

    [Fact]
    public async Task Identificar_NFePuraMod55_RetornaNFe()
    {
        await using var s = Xml(
            $"<NFe xmlns=\"{Ns}\"><infNFe><ide><mod>55</mod></ide></infNFe></NFe>");

        var tipo = await IdentificadorXmlFiscal.IdentificarAsync(s, TestContext.Current.CancellationToken);

        tipo.Should().Be(TipoDocumentoFiscalXml.NFe);
    }

    [Fact]
    public async Task Identificar_ProcEventoNFe_RetornaProcEventoNFe()
    {
        await using var s = Xml(
            $"<procEventoNFe versao=\"1.00\" xmlns=\"{Ns}\"><evento><infEvento><tpEvento>110111</tpEvento></infEvento></evento></procEventoNFe>");

        var tipo = await IdentificadorXmlFiscal.IdentificarAsync(s, TestContext.Current.CancellationToken);

        tipo.Should().Be(TipoDocumentoFiscalXml.ProcEventoNFe);
    }

    [Fact]
    public async Task Identificar_EnvelopeSerpro_RetornaNFeLogSerpro()
    {
        // Wrapper SERPRO reconhecido antes de qualquer <mod>, mesmo com NFe mod 55 embutida.
        await using var s = Xml(
            "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body>" +
            "<nfeConsultaNFeLogResult><retConsNFeLog versao=\"1.00\"><NFeLog>" +
            $"<nfeProc><NFe><infNFe><ide><mod>55</mod></ide></infNFe></NFe></nfeProc>" +
            "</NFeLog></retConsNFeLog></nfeConsultaNFeLogResult></soap:Body></soap:Envelope>");

        var tipo = await IdentificadorXmlFiscal.IdentificarAsync(s, TestContext.Current.CancellationToken);

        tipo.Should().Be(TipoDocumentoFiscalXml.NFeLogSerpro);
    }

    [Fact]
    public async Task Identificar_StreamVazio_RetornaDesconhecido()
    {
        await using var s = Xml("");

        var tipo = await IdentificadorXmlFiscal.IdentificarAsync(s, TestContext.Current.CancellationToken);

        tipo.Should().Be(TipoDocumentoFiscalXml.Desconhecido);
    }

    [Fact]
    public async Task Identificar_XmlMalformado_RetornaDesconhecido()
    {
        await using var s = Xml("<nfeProc><NFe>"); // não fechado

        var tipo = await IdentificadorXmlFiscal.IdentificarAsync(s, TestContext.Current.CancellationToken);

        tipo.Should().Be(TipoDocumentoFiscalXml.Desconhecido);
    }

    [Fact]
    public async Task Identificar_EventoNFe_RetornaEventoNFe()
    {
        await using var s = Xml(
            $"<eventoNFe versao=\"1.00\" xmlns=\"{Ns}\"><infEvento><tpEvento>110111</tpEvento></infEvento></eventoNFe>");

        var tipo = await IdentificadorXmlFiscal.IdentificarAsync(s, TestContext.Current.CancellationToken);

        tipo.Should().Be(TipoDocumentoFiscalXml.EventoNFe);
    }

    [Fact]
    public async Task Identificar_EnvEvento_RetornaEventoNFe()
    {
        await using var s = Xml(
            $"<envEvento versao=\"1.00\" xmlns=\"{Ns}\"><evento><infEvento><tpEvento>110110</tpEvento></infEvento></evento></envEvento>");

        var tipo = await IdentificadorXmlFiscal.IdentificarAsync(s, TestContext.Current.CancellationToken);

        tipo.Should().Be(TipoDocumentoFiscalXml.EventoNFe);
    }

    [Fact]
    public async Task Identificar_NFCePuraMod65_RetornaNFCe()
    {
        await using var s = Xml(
            $"<NFe xmlns=\"{Ns}\"><infNFe><ide><mod>65</mod></ide></infNFe></NFe>");

        var tipo = await IdentificadorXmlFiscal.IdentificarAsync(s, TestContext.Current.CancellationToken);

        tipo.Should().Be(TipoDocumentoFiscalXml.NFCe);
    }

    [Fact]
    public async Task Identificar_ComBomEDeclaracaoXml_RetornaNFeProc()
    {
        // Arquivos reais da SEFAZ vêm com declaração <?xml?> e às vezes BOM UTF-8.
        var conteudo = $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><nfeProc versao=\"4.00\" xmlns=\"{Ns}\"><NFe><infNFe><ide><mod>55</mod></ide></infNFe></NFe></nfeProc>";
        byte[] bom = Encoding.UTF8.GetPreamble();
        byte[] corpo = Encoding.UTF8.GetBytes(conteudo);
        await using var s = new MemoryStream([.. bom, .. corpo]);

        var tipo = await IdentificadorXmlFiscal.IdentificarAsync(s, TestContext.Current.CancellationToken);

        tipo.Should().Be(TipoDocumentoFiscalXml.NFeProc);
    }
}

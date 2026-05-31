using TecnoFisc.Sped.Core.Enums;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Core.Xml;
using TecnoFisc.Sped.NFeNFCe.Enums;

namespace TecnoFisc.Sped.NFeNFCe.Tests;

/// <summary>
/// Testes end-to-end de leitura da slice 14.3 (piloto NF-e 55): valida o parser
/// order-independent contra fixtures anonimizadas — o XML canônico (<c>nfeProc</c>) e o
/// envelope SERPRO (<c>NFeLog</c> com elementos reordenados, atributos convertidos em elementos,
/// <c>protNFe</c> antes da <c>NFe</c> e eventos embutidos).
/// </summary>
public sealed class NFeParseE2ETests
{
    private const string ChaveEsperada = "31210711222333000181550050007156531436322400";

    private static async Task<NFe> LerFixtureAsync(string nome, CancellationToken ct)
    {
        var caminho = Path.Combine(AppContext.BaseDirectory, "Fixtures", nome);
        await using var stream = File.OpenRead(caminho);
        var parser = new ParserNFe();
        return await parser.ReadNFeAsync(stream, ct);
    }

    [Fact]
    public async Task LeNFeCanonica_CamposEssenciais()
    {
        var nfe = await LerFixtureAsync("nfe55-icms60-canonico.xml", TestContext.Current.CancellationToken);

        nfe.ChaveAcesso.Should().Be(ChaveAcesso.Create(ChaveEsperada));
        nfe.Versao.Should().Be("4.00");

        nfe.Ide.Mod.Codigo.Should().Be("55");
        nfe.Ide.NNF.Should().Be(715653);
        nfe.Ide.Serie.Should().Be(5);
        nfe.Ide.CNF.Should().Be("43632240");
        nfe.Ide.NatOp.Should().Be("TRANSF.PROD.O.U/TERC");
        nfe.Ide.TpNF.Should().Be(IndicadorOperacao.Saida);
        nfe.Ide.TpAmb.Should().Be(TipoAmbiente.Producao);
        nfe.Ide.FinNFe.Should().Be(FinalidadeEmissao.Normal);
        nfe.Ide.TpEmis.Should().Be(TipoEmissao.Normal);
        nfe.Ide.IndPres.Should().Be(IndicadorPresenca.NaoPresencialOutros);
        nfe.Ide.IndIntermed.Should().Be(IndicadorIntermediador.SemIntermediador);
        nfe.Ide.IndFinal.Should().BeFalse();
        nfe.Ide.CMunFG.ToString().Should().Be("3168606");
        nfe.Ide.DhEmi.Should().Be(new DateTimeOffset(2021, 7, 19, 10, 38, 40, TimeSpan.FromHours(-3)));

        nfe.Emit.CNPJ.ToString().Should().Be("11222333000181");
        nfe.Emit.XNome.Should().Be("EMPRESA TESTE LTDA");
        nfe.Emit.CRT.Should().Be(3);
        nfe.Emit.EnderEmit.UF.Should().Be("MG");
        nfe.Emit.EnderEmit.CMun.ToString().Should().Be("3168606");

        nfe.Dest.Should().NotBeNull();
        nfe.Dest!.CNPJ.ToString().Should().Be("11444777000161");
        nfe.Dest.XNome.Should().Be("CLIENTE TESTE LTDA");
        nfe.Dest.IndIEDest.Should().Be(1);
    }

    [Fact]
    public async Task LeNFeCanonica_ItensComIcms60()
    {
        var nfe = await LerFixtureAsync("nfe55-icms60-canonico.xml", TestContext.Current.CancellationToken);

        nfe.Itens.Should().HaveCount(2);

        var item1 = nfe.Itens[0];
        item1.NItem.Should().Be(1);
        item1.Prod.CProd.Should().Be("0000988");
        item1.Prod.XProd.Should().Be("BRAHMA CHOPP 600ML");
        item1.Prod.NCM.ToString().Should().Be("22030000");
        item1.Prod.CEST!.Value.ToString().Should().Be("0302100");
        item1.Prod.CFOP.ToString().Should().Be("5409");
        item1.Prod.CEAN.ToString().Should().Be("7891149010400");
        item1.Prod.QCom.Should().Be(336.0000m);
        item1.Prod.VProd.Should().Be(14567.21m);
        item1.Prod.IndTot.Should().BeTrue();

        var icms = item1.Imposto.Icms.Should().BeOfType<Icms60>().Subject;
        icms.Orig.Should().Be(OrigemMercadoria.Nacional);
        icms.CST.ToString().Should().Be("060");
        icms.PST.Should().Be(18.00m);
        icms.VICMSSubstituto.Should().Be(1415.76m);

        nfe.Itens[1].Prod.VProd.Should().Be(54281.08m);
    }

    [Fact]
    public async Task LeNFeCanonica_TotaisEProtocolo()
    {
        var nfe = await LerFixtureAsync("nfe55-icms60-canonico.xml", TestContext.Current.CancellationToken);

        nfe.Total.ICMSTot.VProd.Should().Be(68848.29m);
        nfe.Total.ICMSTot.VNF.Should().Be(68848.29m);
        nfe.Total.ICMSTot.VICMS.Should().Be(0m);

        nfe.IsAutorizada.Should().BeTrue();
        nfe.Protocolo.Should().NotBeNull();
        nfe.Protocolo!.CStat.Should().Be(100);
        nfe.Protocolo.IsAutorizada.Should().BeTrue();
        nfe.Protocolo.NProt.Should().Be("131214250732778");
        nfe.Protocolo.ChNFe.Should().Be(ChaveAcesso.Create(ChaveEsperada));
    }

    [Fact]
    public async Task LeNFeSerpro_DesembrulhaEnvelopeEIgnoraEventos()
    {
        var nfe = await LerFixtureAsync("nfe55-icms60-serpro.xml", TestContext.Current.CancellationToken);

        // Desembrulho do envelope SOAP/NFeLog + protNFe antes da NFe + Id/nItem como elemento.
        nfe.ChaveAcesso.Should().Be(ChaveAcesso.Create(ChaveEsperada));
        nfe.Itens.Should().HaveCount(2);
        nfe.Itens[0].NItem.Should().Be(1);
        nfe.Itens[1].NItem.Should().Be(2);
        nfe.IsAutorizada.Should().BeTrue();
        nfe.Protocolo!.CStat.Should().Be(100);
    }

    [Fact]
    public async Task CanonicoESerpro_ProduzemOsMesmosValores()
    {
        var ct = TestContext.Current.CancellationToken;
        var canonico = await LerFixtureAsync("nfe55-icms60-canonico.xml", ct);
        var serpro = await LerFixtureAsync("nfe55-icms60-serpro.xml", ct);

        // A desserialização order-independent deve render o mesmo modelo nos dois formatos.
        serpro.ChaveAcesso.Should().Be(canonico.ChaveAcesso);
        serpro.Ide.Should().Be(canonico.Ide);
        serpro.Emit.Should().Be(canonico.Emit);
        serpro.Dest.Should().Be(canonico.Dest);
        serpro.Total.Should().Be(canonico.Total);
        serpro.Itens.Should().BeEquivalentTo(canonico.Itens);
    }

    [Fact]
    public async Task ReadAsync_RetornaContratoComum()
    {
        var caminho = Path.Combine(AppContext.BaseDirectory, "Fixtures", "nfe55-icms60-canonico.xml");
        await using var stream = File.OpenRead(caminho);
        var parser = new ParserNFe();

        IDocumentoFiscalXml documento = await parser.ReadAsync(stream, TestContext.Current.CancellationToken);

        documento.Should().BeOfType<NFe>();
        documento.ChaveAcesso.Should().Be(ChaveAcesso.Create(ChaveEsperada));
    }
}

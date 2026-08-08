using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Gerador;

namespace TecnoFisc.Sped.Ecf.Tests.Versionamento;

public sealed class RegistrosRemovidosLeiaute11Tests
{
    [Fact]
    public void Catalogo_ConheceX300ComoDescontinuadoNoLeiaute11()
    {
        new CatalogoSpedGerado().TentarObter("X300", out var metadados).Should().BeTrue();

        metadados!.Bloco.Should().Be("X");
        metadados.DescontinuadoEm.Should().Be((int)LayoutEcf.V011);
        metadados.IntroduzidoEm.Should().Be(0);
        metadados.Campos.Should().BeEmpty();
    }

    [Fact]
    public async Task Leitura_DeLeiaute10ComX300_NaoAborta()
    {
        var registros = await FixtureEcf.ReadAsync(10, "|X300|000001|EXPORTACAO|1234,56|");

        registros.Should().ContainSingle(registro => registro.Codigo == "X300");
        registros.OfType<RegistroNaoReconhecido>().Should().BeEmpty();
    }

    internal static readonly string[] CodigosRemovidos =
        ["X291", "X300", "X305", "X310", "X320", "X325", "X330"];

    /// <summary>
    /// Nível hierárquico de cada um dos sete, extraído diretamente do Manual ECF Leiaute 10
    /// (Anexo ADE Cofis nº 59/2023): X291 p.450, X300 p.456, X305 p.471, X310 p.473, X320 p.477,
    /// X325 p.490, X330 p.492. Nenhum precisou do fallback por inferência de vizinho (X292).
    /// </summary>
    internal static readonly IReadOnlyDictionary<string, int> NiveisEsperados =
        new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["X291"] = 2,
            ["X300"] = 2,
            ["X305"] = 3,
            ["X310"] = 3,
            ["X320"] = 2,
            ["X325"] = 3,
            ["X330"] = 3,
        };

    [Fact]
    public void Catalogo_ConheceOsSeteRemovidosENenhumTemCampoModelado()
    {
        var catalogo = new CatalogoSpedGerado();

        foreach (var codigo in CodigosRemovidos)
        {
            catalogo.TentarObter(codigo, out var metadados).Should().BeTrue($"{codigo} precisa ser reconhecido");
            metadados!.DescontinuadoEm.Should().Be((int)LayoutEcf.V011, $"{codigo} saiu no leiaute 11");
            metadados.Bloco.Should().Be("X");
            metadados.Campos.Should().BeEmpty($"{codigo} não tem campos modelados nesta versão");
            metadados.Nivel.Should().Be(NiveisEsperados[codigo], $"{codigo} deve bater com o manual do leiaute 10");
        }
    }

    [Fact]
    public void Catalogo_TemOsCentoEOitentaDoLeiaute12MaisOsSeteRemovidos()
        => new CatalogoSpedGerado().EnumerarRegistros().Should().HaveCount(187);

    /// <summary>
    /// Prova a alcançabilidade do caminho de escrita a partir da API pública do pacote
    /// TecnoFisc.Sped.Ecf: <c>CatalogoSpedGerado</c> (namespace <c>TecnoFisc.Sped.Ecf.Generated</c>)
    /// e <c>RegistroX300</c> são públicos, e <c>EscritorSpedTxt</c> (TecnoFisc.Sped.Txt.Engine)
    /// chega ao consumidor como dependência transitiva normal — o csproj do ECF referencia
    /// Txt.Engine como ProjectReference comum, não como analyzer-only. Um consumidor externo
    /// então consegue instanciar exatamente esta combinação. Por isso escrever um registro sem
    /// campos modelados precisa falhar alto (ver EscritorSpedTxt.EscreverLinha) em vez de emitir
    /// uma linha mutilada como "|X300|".
    /// </summary>
    [Fact]
    public async Task Escrita_DeX300_LancaEmVezDeEmitirLinhaMutilada()
    {
        var escritor = new EscritorSpedTxt(new CatalogoSpedGerado());
        using var saida = new MemoryStream();

        var act = async () => await escritor.WriteAsync(saida, new RegistroSped[] { new RegistroX300() });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*X300*não tem campos modelados*");
    }

    /// <summary>
    /// O <c>Nivel</c> dos sete veio da leitura do manual em PDF e até aqui só era conferido contra
    /// o catálogo — conferir metadado contra metadado não prova nada sobre a árvore. Este teste lê
    /// um arquivo real e assere a vinculação: X305 e X310 (nível 3) penduram em X300 (nível 2),
    /// X325 e X330 (nível 3) penduram em X320 (nível 2), e os três de nível 2 (X291, X300, X320)
    /// penduram no X001 (nível 1). Um nível errado em qualquer um deles reparenta a subárvore em
    /// silêncio.
    /// </summary>
    [Fact]
    public async Task Leitura_DeLeiaute10_PenduraOsFilhosNosPaisCertos()
    {
        var registros = await FixtureEcf.ReadAsync(
            10,
            "|X001|0|",
            "|X291|000001|AJUSTE|",
            "|X300|000001|EXPORTACAO|1234,56|",
            "|X305|000001|DETALHE|",
            "|X310|000001|DETALHE|",
            "|X320|000002|IMPORTACAO|7890,12|",
            "|X325|000002|DETALHE|",
            "|X330|000002|DETALHE|");

        var porCodigo = registros.ToDictionary(registro => registro.Codigo, StringComparer.Ordinal);
        registros.OfType<RegistroNaoReconhecido>().Should().BeEmpty();

        porCodigo["X291"].Pai.Should().BeSameAs(porCodigo["X001"]);
        porCodigo["X291"].Filhos.Should().BeEmpty("X291 é nível 2 e não abre subárvore");
        porCodigo["X300"].Pai.Should().BeSameAs(porCodigo["X001"]);
        porCodigo["X320"].Pai.Should().BeSameAs(porCodigo["X001"]);
        porCodigo["X305"].Pai.Should().BeSameAs(porCodigo["X300"]);
        porCodigo["X310"].Pai.Should().BeSameAs(porCodigo["X300"]);
        porCodigo["X325"].Pai.Should().BeSameAs(porCodigo["X320"]);
        porCodigo["X330"].Pai.Should().BeSameAs(porCodigo["X320"]);

        porCodigo["X300"].Filhos.Select(filho => filho.Codigo).Should().Equal("X305", "X310");
        porCodigo["X320"].Filhos.Select(filho => filho.Codigo).Should().Equal("X325", "X330");
        porCodigo["X001"].Filhos.Select(filho => filho.Codigo).Should().Equal("X291", "X300", "X320");
    }
}

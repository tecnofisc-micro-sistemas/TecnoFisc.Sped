using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdContribuicoes.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Parser;

/// <summary>
/// Prove o caso real do FiscTax ponta a ponta: C100.ChvNfe com DV inválido não aborta o
/// arquivo em modo leniente. Campo fica null, erro acumulado com ValorBruto.
///
/// A linha C100 usa os campos na ordem declarada em <see cref="RegistroC100"/>:
/// |C100|IndOper|IndEmit|CodPart|CodMod|CodSit|Ser|NumDoc|ChvNfe|...
/// Ordem 2=IndOper, 3=IndEmit, 4=CodPart, 5=CodMod, 6=CodSit, 7=Ser, 8=NumDoc, 9=ChvNfe.
/// A linha é truncada após ChvNfe: campos restantes (DtDoc, VlDoc, etc.) ficam em default —
/// comportamento intencional em modo leniente pois só importa que ChvNfe seja o único erro.
/// </summary>
public sealed class ParserEfdContribuicoesLenienteTests
{
    private static MemoryStream Fluxo(string conteudo)
        => new(EncodingSped.Latin1.GetBytes(conteudo));

    /// <summary>
    /// Chave NF-e com 44 dígitos mas DV mod-11 inválido: dado sujo recebido de terceiros,
    /// exatamente o caso real do FiscTax que motivou o parsing tolerante.
    /// </summary>
    private const string ChaveInvalida = "32251100011998216756550252411200013640116541";

    /// <summary>
    /// Linha C100 com ChvNfe inválida na posição 9 (Ordem=9 do leiaute V006).
    /// Campos antes de ChvNfe têm valores válidos; campos após são omitidos (serão default).
    /// Estrutura: |C100|IndOper|IndEmit|CodPart|CodMod|CodSit|Ser|NumDoc|ChvNfe|
    /// </summary>
    private static string LinhaC100 =>
        $"|C100|0|1|FORN|55|00|001|000000001|{ChaveInvalida}|";

    [Fact]
    public async Task ReadStreamingAsync_Leniente_ChvNfeInvalida_NaoAbortaEAcumulaErro()
    {
        // Envelope mínimo: 0000 + C001 + C100 + C990 + 9999.
        // A linha 0000 tem 13 campos (CodVer|TipoEscrit|IndSitEsp|NumRecAnterior|DtIni|DtFin|Nome|Cnpj|Uf|CodMun|Suframa|IndNatPJ|IndAtiv).
        var sped =
            "|0000|006|0|||01012025|31012025|EMPRESA LTDA|11222333000181|SP|3550308||00|2|\r\n" +
            "|C001|0|\r\n" +
            LinhaC100 + "\r\n" +
            "|C990|3|\r\n" +
            "|9999|5|\r\n";

        var parser = new ParserEfdContribuicoes(new ReadingOptions { LenientFieldParsing = true });
        var registros = new List<RegistroSped>();
        await foreach (var r in parser.ReadStreamingAsync(Fluxo(sped), TestContext.Current.CancellationToken))
            registros.Add(r);

        // O arquivo não deve abortar: todos os 5 registros devem ser emitidos.
        registros.Should().HaveCount(5);

        var c100 = registros.OfType<RegistroC100>().Single();
        // ChvNfe é nullable: com DV inválido, campo fica null (default).
        c100.ChvNfe.Should().BeNull();
        // O único erro acumulado deve ser ChvNfe.
        c100.ErrosDeFormato.Should().ContainSingle(e => e.Campo == "ChvNfe");
        c100.ErrosDeFormato.Single(e => e.Campo == "ChvNfe").ValorBruto.Should().Be(ChaveInvalida);
    }

    [Fact]
    public void ParseLinha_DelegaAoLeitor_RetornaRegistroComErroChvNfe()
    {
        // ParseLinha funciona sempre em modo leniente (força forcarLenienteCampo=true internamente).
        var parser = new ParserEfdContribuicoes();

        var resultado = parser.ParseLinha(LinhaC100);

        resultado.Sucesso.Should().BeTrue();
        var c100 = resultado.Valor.Should().BeOfType<RegistroC100>().Subject;
        c100.ErrosDeFormato.Should().ContainSingle(e => e.Campo == "ChvNfe");
        c100.ErrosDeFormato.Single(e => e.Campo == "ChvNfe").ValorBruto.Should().Be(ChaveInvalida);
    }
}

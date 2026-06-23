using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Core.Erros;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Parser;

public sealed class LeitorSpedTxtLenienteTests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000Sintetico).Assembly);

    private static MemoryStream FluxoSped(string conteudo)
        => new(EncodingSped.Latin1.GetBytes(conteudo));

    private static async Task<List<RegistroSped>> LerAsync(string conteudo, ReadingOptions opcoes)
    {
        var leitor = new LeitorSpedTxt(_catalogo, opcoes);
        var resultado = new List<RegistroSped>();
        await foreach (var r in leitor.ReadStreamingAsync(FluxoSped(conteudo)))
            resultado.Add(r);
        return resultado;
    }

    private static ReadingOptions Leniente => new() { LenientFieldParsing = true };

    [Fact]
    public async Task Estrito_CampoNumericoMalformado_ContinuaLancando()
    {
        // Comportamento atual preservado: default (estrito) lança no primeiro erro de campo.
        var act = async () => await LerAsync("|C001|abc|\r\n", ReadingOptions.Default);

        await act.Should().ThrowAsync<ErroFormatoSpedException>();
    }

    [Fact]
    public async Task Leniente_CampoMalformado_EmiteRegistroComCampoNoDefaultEAcumulaErro()
    {
        var registros = await LerAsync(
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|C001|abc|\r\n" +
            "|9999|3|\r\n", Leniente);

        registros.Select(r => r.Codigo).Should().Equal(["0000", "C001", "9999"]);

        var c001 = registros.OfType<RegistroC001Sintetico>().Single();
        c001.IndMov.Should().Be(default);                       // campo falho permanece no default
        c001.ErrosDeFormato.Should().HaveCount(1);
        c001.ErrosDeFormato[0].Campo.Should().Be("IndMov");
        c001.ErrosDeFormato[0].CodigoRegistro.Should().Be("C001");
        c001.ErrosDeFormato[0].ValorBruto.Should().Be("abc");
    }

    [Fact]
    public async Task Leniente_DoisCamposRuinsNaMesmaLinha_AcumulaAmbosSemAbortar()
    {
        // C100: |IND_OPER|COD_PART|VL_DOC|CFOP| — COD_PART e VL_DOC malformados.
        var registros = await LerAsync(
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|C100|0|abc|xyz|5102|\r\n" +
            "|9999|3|\r\n", Leniente);

        var c100 = registros.OfType<RegistroC100Sintetico>().Single();
        c100.ErrosDeFormato.Should().HaveCount(2);
        c100.ErrosDeFormato.Select(e => e.Campo).Should().Contain(["CodPart", "VlDoc"]);
    }

    [Fact]
    public async Task Leniente_CaminhoFeliz_NaoAlocaListaDeErros()
    {
        var registros = await LerAsync(
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|9999|2|\r\n", Leniente);

        registros.Should().OnlyContain(r => r.ErrosDeFormato.Count == 0);
        // instância vazia compartilhada (lista lazy não inicializada)
        registros[0].ErrosDeFormato.Should().BeSameAs(registros[1].ErrosDeFormato);
    }

    [Fact]
    public async Task Leniente_UmaLinhaRuim_NaoDerrubaAsDemais()
    {
        var registros = await LerAsync(
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|C001|abc|\r\n" +
            "|C100|0|123|1500,75|5102|\r\n" +
            "|9999|4|\r\n", Leniente);

        registros.Select(r => r.Codigo).Should().Equal(["0000", "C001", "C100", "9999"]);
        registros.OfType<RegistroC100Sintetico>().Single().CodPart.Should().Be(123);
    }
}

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco0;

/// <summary>
/// Invariante de round-trip do <see cref="Registro0000"/>: parse → gerar → texto idêntico.
/// Usa o leitor/escritor reais do Core sobre o catálogo da EFD Contribuições, sem fixtures
/// sintéticos. Cobre o registro com e sem campos opcionais para garantir que o tratamento
/// de "||" preserve a forma do arquivo.
/// </summary>
public sealed class Registro0000RoundTripTests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000).Assembly);

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerStreamingAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }

    [Fact]
    public async Task EscriturarOriginalSemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped =
            "|0000|006|0|||01012025|31012025|EMPRESA TESTE LTDA|11222333000181|SP|3550308||00|2|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task EscriturarRetificadora_ComNumRecAnteriorESituacaoEspecial_PreservaTextoCanonico()
    {
        const string sped =
            "|0000|006|1|3|RECABCDEF1234567890|01032025|31032025|EMPRESA INCORPORADA|11222333000181|SP|3550308|987654321|03|0|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task ParseGenerate_PreservaIdentidadeSemanticaDoModelo()
    {
        const string sped =
            "|0000|006|0|||01012025|31012025|EMPRESA TESTE|11222333000181|SP|3550308||00|2|\r\n";

        var ct = TestContext.Current.CancellationToken;
        var leitor = new LeitorSpedTxt(_catalogo);
        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));

        Registro0000? registro = null;
        await foreach (var r in leitor.LerStreamingAsync(entrada, ct))
            registro = (Registro0000)r;

        registro.Should().NotBeNull();
        registro!.CodVer.Should().Be("006");
        registro.TipoEscrit.Should().Be(TipoEscrituracao.Original);
        registro.IndSitEsp.Should().BeNull();
        registro.NumRecAnterior.Should().BeNull();
        registro.DtIni.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtFin.Should().Be(new DateOnly(2025, 1, 31));
        registro.Nome.Should().Be("EMPRESA TESTE");
        registro.Cnpj.Should().Be(Cnpj.Criar("11222333000181"));
        registro.Uf.Should().Be("SP");
        registro.CodMun.Should().Be("3550308");
        registro.Suframa.Should().BeNull();
        registro.IndNatPJ.Should().Be(IndicadorNaturezaPessoaJuridica.PessoaJuridicaEmGeral);
        registro.IndAtiv.Should().Be(IndicadorAtividade.Comercio);
    }

    [Fact]
    public async Task ParseGenerate_PreservaCaracteresLatin1NoNomeEmpresarial()
    {
        const string sped =
            "|0000|006|0|||01012025|31012025|AÇÃO COMÉRCIO LTDA|11222333000181|SP|3550308||00|2|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

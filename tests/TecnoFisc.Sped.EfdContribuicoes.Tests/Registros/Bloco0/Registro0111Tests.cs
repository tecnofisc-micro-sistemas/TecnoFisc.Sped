using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 4.006 — exercita <see cref="Registro0111"/> contra o Guia Prático v1.35 (p. 74):
/// metadados de catálogo, mapeamento de campos decimais e invariante de round-trip.
/// </summary>
public sealed class Registro0111Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0111).Assembly);

    [Fact]
    public void Atributo_DeclaraCodigo0111_Nivel3_Bloco0()
    {
        var atributo = typeof(Registro0111).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0111");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0111ComCincoCamposNaOrdem()
    {
        _catalogo.TentarObter("0111".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0111");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "RecBruNcumTribMi",
            "RecBruNcumNtMi",
            "RecBruNcumExp",
            "RecBruCum",
            "RecBruTotal",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6]);
    }

    [Fact]
    public void Catalogo_TodosCamposTem2DecimaisEObrigatorio()
    {
        _catalogo.TentarObter("0111".AsSpan(), out var meta);

        meta!.Campos.Should().AllSatisfy(c =>
        {
            c.Decimais.Should().Be(2);
            c.Obrigatorio.Should().BeTrue();
        });
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0111".AsSpan(), out var meta);
        var registro = (Registro0111)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "100000,00".AsSpan());
        meta.Campos[1].Definidor(registro, "50000,00".AsSpan());
        meta.Campos[2].Definidor(registro, "25000,00".AsSpan());
        meta.Campos[3].Definidor(registro, "30000,00".AsSpan());
        meta.Campos[4].Definidor(registro, "205000,00".AsSpan());

        registro.RecBruNcumTribMi.Should().Be(100000.00m);
        registro.RecBruNcumNtMi.Should().Be(50000.00m);
        registro.RecBruNcumExp.Should().Be(25000.00m);
        registro.RecBruCum.Should().Be(30000.00m);
        registro.RecBruTotal.Should().Be(205000.00m);
    }

    [Fact]
    public void Serializar_CamposDecimais_UsaVirgulaSPED()
    {
        _catalogo.TentarObter("0111".AsSpan(), out var meta);
        var registro = (Registro0111)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1234567,89".AsSpan());

        meta.Campos[0].Serializar(registro).Should().Be("1234567,89");
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Empresa não-cumulativa com receitas em todos os segmentos; total = soma dos campos 02-05.
        const string sped = "|0111|100000,00|50000,00|25000,00|30000,00|205000,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SomenteReceitaCumulativa_PreservaTextoCanonico()
    {
        // Empresa somente cumulativa: campos não-cumulativos zerados.
        const string sped = "|0111|0,00|0,00|0,00|500000,00|500000,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_AninhadoSob0110_RespeitaHierarquia()
    {
        const string sped =
            "|0000|006|0|||01012025|31012025|EMPRESA SA|11222333000181|SP|3550308||00|2|\r\n" +
            "|0001|0|\r\n" +
            "|0110|3|2|1||\r\n" +
            "|0111|100000,00|50000,00|25000,00|30000,00|205000,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

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
}

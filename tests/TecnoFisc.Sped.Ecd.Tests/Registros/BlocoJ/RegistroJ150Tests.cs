using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoJ;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoJ;

/// <summary>
/// Sub-stage 10.048 — exercita a forma do <see cref="RegistroJ150"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 181–186): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroJ150Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000).Assembly);

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.ReadStreamingAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.WriteAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }

    [Fact]
    public void Atributo_DeclaraJ150_Nivel3_BlocoJ()
    {
        var atributo = typeof(RegistroJ150).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("J150");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("J");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroJ150Com12CamposNaOrdem()
    {
        _catalogo.TentarObter("J150".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("J150");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "NuOrdem", "CodAgl", "IndCodAgl", "NivelAgl", "CodAglSup",
            "DescrCodAgl",
            "VlCtaIni", "IndDcCtaIni",
            "VlCtaFin", "IndDcCtaFin",
            "IndGrpDre", "NotaExpRef",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("J150".AsSpan(), out var meta);
        var registro = (RegistroJ150)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "20".AsSpan());                          // NuOrdem
        meta.Campos[1].Definidor(registro, "3.3".AsSpan());                         // CodAgl
        meta.Campos[2].Definidor(registro, "T".AsSpan());                           // IndCodAgl
        meta.Campos[3].Definidor(registro, "2".AsSpan());                           // NivelAgl
        meta.Campos[4].Definidor(registro, "3".AsSpan());                           // CodAglSup
        meta.Campos[5].Definidor(registro, "DESPESAS OPERACIONAIS".AsSpan());       // DescrCodAgl
        meta.Campos[6].Definidor(registro, "10000,00".AsSpan());                    // VlCtaIni
        meta.Campos[7].Definidor(registro, "D".AsSpan());                           // IndDcCtaIni
        meta.Campos[8].Definidor(registro, "936844,99".AsSpan());                   // VlCtaFin
        meta.Campos[9].Definidor(registro, "D".AsSpan());                           // IndDcCtaFin
        meta.Campos[10].Definidor(registro, "D".AsSpan());                          // IndGrpDre
        meta.Campos[11].Definidor(registro, "233".AsSpan());                        // NotaExpRef

        registro.NuOrdem.Should().Be(20);
        registro.CodAgl.Should().Be("3.3");
        registro.IndCodAgl.Should().Be(IndicadorTipoCodigoAglutinacao.Totalizador);
        registro.NivelAgl.Should().Be(2);
        registro.CodAglSup.Should().Be("3");
        registro.DescrCodAgl.Should().Be("DESPESAS OPERACIONAIS");
        registro.VlCtaIni.Should().Be(10000m);
        registro.IndDcCtaIni.Should().Be(IndicadorDebitoCredito.Devedor);
        registro.VlCtaFin.Should().Be(936844.99m);
        registro.IndDcCtaFin.Should().Be(IndicadorDebitoCredito.Devedor);
        registro.IndGrpDre.Should().Be(IndicadorGrupoDre.Despesa);
        registro.NotaExpRef.Should().Be("233");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("J150".AsSpan(), out var meta);
        var registro = (RegistroJ150)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);   // CodAgl
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);   // CodAglSup
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);   // VlCtaIni
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);   // IndDcCtaIni
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty);  // NotaExpRef

        registro.CodAgl.Should().BeNull();
        registro.CodAglSup.Should().BeNull();
        registro.VlCtaIni.Should().BeNull();
        registro.IndDcCtaIni.Should().BeNull();
        registro.NotaExpRef.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_TotalizadorComSaldoInicialENotaExplicativa_PreservaTextoCanonico()
    {
        // Totalizador nível 2 — Despesas Operacionais, com saldo inicial e nota explicativa
        const string sped = "|J150|20|3.3|T|2|3|DESPESAS OPERACIONAIS|10000,00|D|936844,99|D|D|233|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ResultadoNivel1SemSaldoAnterior_PreservaTextoCanonico()
    {
        // Nível 1 (Resultado do Exercício): sem COD_AGL_SUP, sem VL_CTA_INI/IND_DC_CTA_INI, sem NOTA_EXP_REF
        const string sped = "|J150|16|4|T|1||Resultado do Periodo|20000,00|C|14650,00|C|R||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_DetalheReceita_PreservaTextoCanonico()
    {
        // Detalhe nível 5 — Receita de Vendas (natureza receita)
        const string sped = "|J150|2|4000|D|5|4.3|Receita de Vendas|40000,00|C|80000,00|C|R||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

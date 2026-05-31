using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoJ;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoJ;

/// <summary>
/// Sub-stage 10.047 — exercita a forma do <see cref="RegistroJ100"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 175–179): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroJ100Tests
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
    public void Atributo_DeclaraJ100_Nivel3_BlocoJ()
    {
        var atributo = typeof(RegistroJ100).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("J100");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("J");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroJ100Com11CamposNaOrdem()
    {
        _catalogo.TentarObter("J100".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("J100");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodAgl", "IndCodAgl", "NivelAgl", "CodAglSup",
            "IndGrpBal", "DescrCodAgl",
            "VlCtaIni", "IndDcCtaIni",
            "VlCtaFin", "IndDcCtaFin",
            "NotaExpRef",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("J100".AsSpan(), out var meta);
        var registro = (RegistroJ100)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "AT".AsSpan());              // CodAgl
        meta.Campos[1].Definidor(registro, "T".AsSpan());               // IndCodAgl
        meta.Campos[2].Definidor(registro, "1".AsSpan());               // NivelAgl
        meta.Campos[3].Definidor(registro, "".AsSpan());                // CodAglSup
        meta.Campos[4].Definidor(registro, "A".AsSpan());               // IndGrpBal
        meta.Campos[5].Definidor(registro, "Ativo Total".AsSpan());     // DescrCodAgl
        meta.Campos[6].Definidor(registro, "1500000,00".AsSpan());      // VlCtaIni
        meta.Campos[7].Definidor(registro, "D".AsSpan());               // IndDcCtaIni
        meta.Campos[8].Definidor(registro, "1800000,00".AsSpan());      // VlCtaFin
        meta.Campos[9].Definidor(registro, "D".AsSpan());               // IndDcCtaFin
        meta.Campos[10].Definidor(registro, "NE01".AsSpan());           // NotaExpRef

        registro.CodAgl.Should().Be("AT");
        registro.IndCodAgl.Should().Be(IndicadorTipoCodigoAglutinacao.Totalizador);
        registro.NivelAgl.Should().Be(1);
        registro.CodAglSup.Should().BeNull();
        registro.IndGrpBal.Should().Be(IndicadorGrupoBalanco.Ativo);
        registro.DescrCodAgl.Should().Be("Ativo Total");
        registro.VlCtaIni.Should().Be(1500000m);
        registro.IndDcCtaIni.Should().Be(IndicadorDebitoCredito.Devedor);
        registro.VlCtaFin.Should().Be(1800000m);
        registro.IndDcCtaFin.Should().Be(IndicadorDebitoCredito.Devedor);
        registro.NotaExpRef.Should().Be("NE01");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("J100".AsSpan(), out var meta);
        var registro = (RegistroJ100)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);   // CodAglSup
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty);  // NotaExpRef

        registro.CodAglSup.Should().BeNull();
        registro.NotaExpRef.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Totalizador nível 1 — Ativo Total, sem COD_AGL_SUP e sem NOTA_EXP_REF
        const string sped = "|J100|AT|T|1||A|Ativo Total|1500000,00|D|1800000,00|D||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_DetalheComSuperiorENotaExplicativa_PreservaTextoCanonico()
    {
        // Detalhe nível 2 do Ativo com código superior e nota explicativa
        const string sped = "|J100|AT_CIR|D|2|AT|A|Ativo Circulante|800000,00|D|900000,00|D|NE01|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_PassivoPatrimonioLiquido_PreservaTextoCanonico()
    {
        // Totalizador nível 1 — Passivo e Patrimônio Líquido com saldo credor
        const string sped = "|J100|PT|T|1||P|Passivo e Patrimonio Liquido|1500000,00|C|1800000,00|C||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

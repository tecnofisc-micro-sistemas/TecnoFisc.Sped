using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoC;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 10.010 — exercita a forma do <see cref="RegistroC040"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 89–92): metadados de catálogo, mapeamento de campo e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroC040Tests
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
    public void Atributo_DeclaraC040_Nivel2_BlocoC()
    {
        var atributo = typeof(RegistroC040).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C040");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC040Com18CamposNaOrdem()
    {
        _catalogo.TentarObter("C040".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C040");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "HashEcdRec", "DtIniEcdRec", "DtFinEcdRec", "CnpjEcdRec",
            "IndEsc", "CodVerLc", "NumOrd", "NatLivr",
            "IndSitEspEcdRec", "IndNireEcdRec", "IndFinEscEcdRec",
            "TipEcdRec", "CodScpEcdRec", "IdentMfEcdRec", "IndEscConsEcdRec",
            "IndCentralizadaEcdRec", "IndMudancaPcEcdRec", "IndPlanoRefEcdRec",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([
            2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
        ]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C040".AsSpan(), out var meta);
        var registro = (RegistroC040)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "ABCDEF1234567890ABCDEF1234567890ABCDEF12".AsSpan());
        meta.Campos[1].Definidor(registro, "01012020".AsSpan());
        meta.Campos[2].Definidor(registro, "31122020".AsSpan());
        meta.Campos[3].Definidor(registro, "12345678000195".AsSpan());
        meta.Campos[4].Definidor(registro, "G".AsSpan());
        meta.Campos[5].Definidor(registro, "9.00".AsSpan());
        meta.Campos[6].Definidor(registro, "1".AsSpan());
        meta.Campos[7].Definidor(registro, "Livro Diario Geral".AsSpan());
        meta.Campos[8].Definidor(registro, "1".AsSpan());
        meta.Campos[9].Definidor(registro, "0".AsSpan());
        meta.Campos[10].Definidor(registro, "0".AsSpan());
        meta.Campos[11].Definidor(registro, "0".AsSpan());
        meta.Campos[12].Definidor(registro, "11111111000191".AsSpan());
        meta.Campos[13].Definidor(registro, "S".AsSpan());
        meta.Campos[14].Definidor(registro, "S".AsSpan());
        meta.Campos[15].Definidor(registro, "0".AsSpan());
        meta.Campos[16].Definidor(registro, "0".AsSpan());
        meta.Campos[17].Definidor(registro, "1".AsSpan());

        registro.HashEcdRec.Should().Be("ABCDEF1234567890ABCDEF1234567890ABCDEF12");
        registro.DtIniEcdRec.Should().Be(new DateOnly(2020, 1, 1));
        registro.DtFinEcdRec.Should().Be(new DateOnly(2020, 12, 31));
        registro.CnpjEcdRec.Should().NotBeNull();
        registro.CnpjEcdRec!.Value.ToString().Should().Be("12345678000195");
        registro.IndEsc.Should().Be(FormaEscrituracaoContabil.LivroDiarioGeral);
        registro.CodVerLc.Should().Be("9.00");
        registro.NumOrd.Should().Be(1);
        registro.NatLivr.Should().Be("Livro Diario Geral");
        registro.IndSitEspEcdRec.Should().Be(SituacaoEspecial.Cisao);
        registro.IndNireEcdRec.Should().Be(IndicadorExistenciaNire.NaoPossui);
        registro.IndFinEscEcdRec.Should().Be(IndicadorFinalidadeEscrituracao.Original);
        registro.TipEcdRec.Should().Be(TipoEcd.NaoParticipanteScp);
        registro.CodScpEcdRec.Should().NotBeNull();
        registro.CodScpEcdRec!.Value.ToString().Should().Be("11111111000191");
        registro.IdentMfEcdRec.Should().Be(IndicadorSimNao.Sim);
        registro.IndEscConsEcdRec.Should().Be(IndicadorSimNao.Sim);
        registro.IndCentralizadaEcdRec.Should().Be(IndicadorEscrituracaoCentralizada.Centralizada);
        registro.IndMudancaPcEcdRec.Should().Be(IndicadorMudancaPlanoContas.SemMudanca);
        registro.IndPlanoRefEcdRec.Should().Be(CodigoPlanoContasReferencial.PessoaJuridicaGeralLucroReal);
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("C040".AsSpan(), out var meta);
        var registro = (RegistroC040)meta!.Fabrica();

        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);   // IndSitEspEcdRec
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty);  // TipEcdRec
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodScpEcdRec
        meta.Campos[15].Definidor(registro, ReadOnlySpan<char>.Empty);  // IndCentralizadaEcdRec
        meta.Campos[16].Definidor(registro, ReadOnlySpan<char>.Empty);  // IndMudancaPcEcdRec
        meta.Campos[17].Definidor(registro, ReadOnlySpan<char>.Empty);  // IndPlanoRefEcdRec

        registro.IndSitEspEcdRec.Should().BeNull();
        registro.TipEcdRec.Should().BeNull();
        registro.CodScpEcdRec.Should().BeNull();
        registro.IndCentralizadaEcdRec.Should().BeNull();
        registro.IndMudancaPcEcdRec.Should().BeNull();
        registro.IndPlanoRefEcdRec.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C040|ABCDEF1234567890ABCDEF1234567890ABCDEF12|01012020|31122020|12345678000195|G|9.00|1|Livro Diario Geral|1|0|0|0|11111111000191|S|S|0|0|01|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        const string sped =
            "|C040|ABCDEF1234567890ABCDEF1234567890ABCDEF12|01012020|31122020|12345678000195|R|9.00|2|Livro Diario Resumido||1|1|||N|N||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

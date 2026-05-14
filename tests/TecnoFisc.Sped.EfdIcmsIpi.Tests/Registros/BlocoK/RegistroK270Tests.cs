using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 8.205 — exercita a forma do <see cref="RegistroK270"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 259-261): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroK270Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroK270).Assembly);

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
    public void Atributo_DeclaraK270_Nivel3_BlocoK()
    {
        var atributo = typeof(RegistroK270).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K270");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK270ComSeteCamposNaOrdem()
    {
        _catalogo.TentarObter("K270".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K270");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "DtIniAp",
            "DtFinAp",
            "CodOpOs",
            "CodItem",
            "QtdCorPos",
            "QtdCorNeg",
            "Origem",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K270".AsSpan(), out var meta);
        var registro = (RegistroK270)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01012025".AsSpan());
        meta.Campos[1].Definidor(registro, "31012025".AsSpan());
        meta.Campos[2].Definidor(registro, "OP-CORR-001".AsSpan());
        meta.Campos[3].Definidor(registro, "ITEM-CORRIGIDO".AsSpan());
        meta.Campos[4].Definidor(registro, "1,234567".AsSpan());
        meta.Campos[5].Definidor(registro, "0,765432".AsSpan());
        meta.Campos[6].Definidor(registro, "4".AsSpan());

        registro.DtIniAp.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtFinAp.Should().Be(new DateOnly(2025, 1, 31));
        registro.CodOpOs.Should().Be("OP-CORR-001");
        registro.CodItem.Should().Be("ITEM-CORRIGIDO");
        registro.QtdCorPos.Should().Be(1.234567m);
        registro.QtdCorNeg.Should().Be(0.765432m);
        registro.Origem.Should().Be(IndicadorOrigemCorrecaoApontamento.ReprocessamentoReparoConsumoK260K265);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("K270".AsSpan(), out var meta);
        var registro = (RegistroK270)meta!.Fabrica();

        meta!.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.DtIniAp.Should().BeNull();
        registro.DtFinAp.Should().BeNull();
        registro.CodOpOs.Should().BeNull();
        registro.QtdCorPos.Should().BeNull();
        registro.QtdCorNeg.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|K270|01012025|31012025|OP-CORR-001|ITEM-CORRIGIDO|1,234567|0,765432|4|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemPeriodoEQuantidades_PreservaTextoCanonico()
    {
        const string sped = "|K270|||OP-ABERTA|ITEM-CORRIGIDO|||1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("1", IndicadorOrigemCorrecaoApontamento.ProducaoConsumoK230K235)]
    [InlineData("2", IndicadorOrigemCorrecaoApontamento.ProducaoConsumoK250K255)]
    [InlineData("3", IndicadorOrigemCorrecaoApontamento.DesmontagemConsumoK210K215)]
    [InlineData("4", IndicadorOrigemCorrecaoApontamento.ReprocessamentoReparoConsumoK260K265)]
    [InlineData("5", IndicadorOrigemCorrecaoApontamento.MovimentacaoInternaK220)]
    [InlineData("6", IndicadorOrigemCorrecaoApontamento.ProducaoK291)]
    [InlineData("7", IndicadorOrigemCorrecaoApontamento.ConsumoK292)]
    [InlineData("8", IndicadorOrigemCorrecaoApontamento.ProducaoK301)]
    [InlineData("9", IndicadorOrigemCorrecaoApontamento.ConsumoK302)]
    public void Definidor_Origem_MapeiaCodigos(string valor, IndicadorOrigemCorrecaoApontamento esperado)
    {
        _catalogo.TentarObter("K270".AsSpan(), out var meta);
        var registro = (RegistroK270)meta!.Fabrica();

        meta.Campos[6].Definidor(registro, valor.AsSpan());

        registro.Origem.Should().Be(esperado);
    }
}

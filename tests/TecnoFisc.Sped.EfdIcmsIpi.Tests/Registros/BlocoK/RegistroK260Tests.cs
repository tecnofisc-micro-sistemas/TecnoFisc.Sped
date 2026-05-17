using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 8.203 — exercita a forma do <see cref="RegistroK260"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 258-259): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroK260Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroK260).Assembly);

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
    public void Atributo_DeclaraK260_Nivel3_BlocoK()
    {
        var atributo = typeof(RegistroK260).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K260");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK260ComSeisCamposNaOrdem()
    {
        _catalogo.TentarObter("K260".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K260");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodOpOs",
            "CodItem",
            "DtSaida",
            "QtdSaida",
            "DtRet",
            "QtdRet",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K260".AsSpan(), out var meta);
        var registro = (RegistroK260)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "OP-REPROC-001".AsSpan());
        meta.Campos[1].Definidor(registro, "ITEM-REPROCESSADO".AsSpan());
        meta.Campos[2].Definidor(registro, "10012025".AsSpan());
        meta.Campos[3].Definidor(registro, "8,123456".AsSpan());
        meta.Campos[4].Definidor(registro, "20012025".AsSpan());
        meta.Campos[5].Definidor(registro, "7,654321".AsSpan());

        registro.CodOpOs.Should().Be("OP-REPROC-001");
        registro.CodItem.Should().Be("ITEM-REPROCESSADO");
        registro.DtSaida.Should().Be(new DateOnly(2025, 1, 10));
        registro.QtdSaida.Should().Be(8.123456m);
        registro.DtRet.Should().Be(new DateOnly(2025, 1, 20));
        registro.QtdRet.Should().Be(7.654321m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("K260".AsSpan(), out var meta);
        var registro = (RegistroK260)meta!.Fabrica();

        meta!.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodOpOs.Should().BeNull();
        registro.DtRet.Should().BeNull();
        registro.QtdRet.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|K260|OP-REPROC-001|ITEM-REPROCESSADO|10012025|8,123456|20012025|7,654321|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemRetornoInformado_PreservaTextoCanonico()
    {
        const string sped = "|K260||ITEM-REPARO|15012025|3,000000|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 8.199 — exercita a forma do <see cref="RegistroK230"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 254-255): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroK230Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroK230).Assembly);

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
    public void Atributo_DeclaraK230_Nivel3_BlocoK()
    {
        var atributo = typeof(RegistroK230).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K230");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK230ComCincoCamposNaOrdem()
    {
        _catalogo.TentarObter("K230".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K230");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "DtIniOp",
            "DtFinOp",
            "CodDocOp",
            "CodItem",
            "QtdEnc",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K230".AsSpan(), out var meta);
        var registro = (RegistroK230)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "02012025".AsSpan());
        meta.Campos[1].Definidor(registro, "15012025".AsSpan());
        meta.Campos[2].Definidor(registro, "OP-2025-0001".AsSpan());
        meta.Campos[3].Definidor(registro, "ITEM-PRODUZIDO".AsSpan());
        meta.Campos[4].Definidor(registro, "12,345678".AsSpan());

        registro.DtIniOp.Should().Be(new DateOnly(2025, 1, 2));
        registro.DtFinOp.Should().Be(new DateOnly(2025, 1, 15));
        registro.CodDocOp.Should().Be("OP-2025-0001");
        registro.CodItem.Should().Be("ITEM-PRODUZIDO");
        registro.QtdEnc.Should().Be(12.345678m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("K230".AsSpan(), out var meta);
        var registro = (RegistroK230)meta!.Fabrica();

        meta!.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.DtIniOp.Should().BeNull();
        registro.DtFinOp.Should().BeNull();
        registro.CodDocOp.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|K230|02012025|15012025|OP-2025-0001|ITEM-PRODUZIDO|12,345678|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemOrdemDeProducao_PreservaTextoCanonico()
    {
        const string sped = "|K230||||ITEM-PRODUZIDO|25,000000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

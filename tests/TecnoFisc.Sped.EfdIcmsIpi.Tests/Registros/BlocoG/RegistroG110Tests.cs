using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoG;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoG;

/// <summary>
/// Sub-stage 8.181 — exercita a forma do <see cref="RegistroG110"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 237): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroG110Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroG110).Assembly);

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
    public void Atributo_DeclaraG110_Nivel2_BlocoG()
    {
        var atributo = typeof(RegistroG110).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("G110");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("G");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroG110ComNoveCamposNaOrdem()
    {
        _catalogo.TentarObter("G110".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("G110");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "DtIni",
            "DtFin",
            "SaldoInIcms",
            "SomParc",
            "VlTribExp",
            "VlTotal",
            "IndPerSai",
            "IcmsAprop",
            "SomIcmsOc",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("G110".AsSpan(), out var meta);
        var registro = (RegistroG110)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01012025".AsSpan());
        meta.Campos[1].Definidor(registro, "31012025".AsSpan());
        meta.Campos[2].Definidor(registro, "1000,00".AsSpan());
        meta.Campos[3].Definidor(registro, "250,00".AsSpan());
        meta.Campos[4].Definidor(registro, "8000,00".AsSpan());
        meta.Campos[5].Definidor(registro, "10000,00".AsSpan());
        meta.Campos[6].Definidor(registro, "0,80000000".AsSpan());
        meta.Campos[7].Definidor(registro, "200,00".AsSpan());
        meta.Campos[8].Definidor(registro, "50,00".AsSpan());

        registro.DtIni.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtFin.Should().Be(new DateOnly(2025, 1, 31));
        registro.SaldoInIcms.Should().Be(1000.00m);
        registro.SomParc.Should().Be(250.00m);
        registro.VlTribExp.Should().Be(8000.00m);
        registro.VlTotal.Should().Be(10000.00m);
        registro.IndPerSai.Should().Be(0.80000000m);
        registro.IcmsAprop.Should().Be(200.00m);
        registro.SomIcmsOc.Should().Be(50.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("G110".AsSpan(), out var meta);
        var registro = (RegistroG110)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.DtIni.Should().Be(default(DateOnly));
        registro.DtFin.Should().Be(default(DateOnly));
        registro.SaldoInIcms.Should().Be(0m);
        registro.SomParc.Should().Be(0m);
        registro.VlTribExp.Should().Be(0m);
        registro.VlTotal.Should().Be(0m);
        registro.IndPerSai.Should().Be(0m);
        registro.IcmsAprop.Should().Be(0m);
        registro.SomIcmsOc.Should().Be(0m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|G110|01012025|31012025|1000,00|250,00|8000,00|10000,00|0,80000000|200,00|50,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComValoresZerados_PreservaTextoCanonico()
    {
        const string sped =
            "|G110|01022025|28022025|0,00|0,00|0,00|0,00|0,00000000|0,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoG;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoG;

/// <summary>
/// Sub-stage 8.183 - exercita a forma do <see cref="RegistroG126"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 241-242): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class RegistroG126Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroG126).Assembly);

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
    public void Atributo_DeclaraG126_Nivel4_BlocoG()
    {
        var atributo = typeof(RegistroG126).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("G126");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("G");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroG126ComOitoCamposNaOrdem()
    {
        _catalogo.TentarObter("G126".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("G126");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "DtIni",
            "DtFim",
            "NumParc",
            "VlParcPass",
            "VlTribOc",
            "VlTotal",
            "IndPerSai",
            "VlParcAprop",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("G126".AsSpan(), out var meta);
        var registro = (RegistroG126)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01012025".AsSpan());
        meta.Campos[1].Definidor(registro, "31012025".AsSpan());
        meta.Campos[2].Definidor(registro, "001".AsSpan());
        meta.Campos[3].Definidor(registro, "1000,00".AsSpan());
        meta.Campos[4].Definidor(registro, "8000,00".AsSpan());
        meta.Campos[5].Definidor(registro, "10000,00".AsSpan());
        meta.Campos[6].Definidor(registro, "0,80000000".AsSpan());
        meta.Campos[7].Definidor(registro, "800,00".AsSpan());

        registro.DtIni.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtFim.Should().Be(new DateOnly(2025, 1, 31));
        registro.NumParc.Should().Be(1);
        registro.VlParcPass.Should().Be(1000.00m);
        registro.VlTribOc.Should().Be(8000.00m);
        registro.VlTotal.Should().Be(10000.00m);
        registro.IndPerSai.Should().Be(0.80000000m);
        registro.VlParcAprop.Should().Be(800.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("G126".AsSpan(), out var meta);
        var registro = (RegistroG126)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.DtIni.Should().Be(default(DateOnly));
        registro.DtFim.Should().Be(default(DateOnly));
        registro.NumParc.Should().Be(0);
        registro.VlParcPass.Should().Be(0m);
        registro.VlTribOc.Should().Be(0m);
        registro.VlTotal.Should().Be(0m);
        registro.IndPerSai.Should().Be(0m);
        registro.VlParcAprop.Should().Be(0m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|G126|01012025|31012025|1|1000,00|8000,00|10000,00|0,80000000|800,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComValoresZerados_PreservaTextoCanonico()
    {
        const string sped =
            "|G126|01022025|28022025|0|0,00|0,00|0,00|0,00000000|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

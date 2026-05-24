using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.067 — exercita a forma do <see cref="RegistroC190"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 102): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC190Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC190).Assembly);

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
    public void Atributo_DeclaraC190_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC190).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C190");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC190Com11CamposNaOrdem()
    {
        _catalogo.TentarObter("C190".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C190");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CstIcms", "Cfop", "AliqIcms",
            "VlOpr", "VlBcIcms", "VlIcms",
            "VlBcIcmsSt", "VlIcmsSt", "VlRedBc",
            "VlIpi", "CodObs"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C190".AsSpan(), out var meta);
        var registro = (RegistroC190)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "060".AsSpan());          // CstIcms
        meta.Campos[1].Definidor(registro, "5201".AsSpan());         // Cfop
        meta.Campos[2].Definidor(registro, "12,00".AsSpan());        // AliqIcms
        meta.Campos[3].Definidor(registro, "15000,00".AsSpan());     // VlOpr
        meta.Campos[4].Definidor(registro, "10000,00".AsSpan());     // VlBcIcms
        meta.Campos[5].Definidor(registro, "1200,00".AsSpan());      // VlIcms
        meta.Campos[6].Definidor(registro, "5000,00".AsSpan());      // VlBcIcmsSt
        meta.Campos[7].Definidor(registro, "600,00".AsSpan());       // VlIcmsSt
        meta.Campos[8].Definidor(registro, "0,00".AsSpan());         // VlRedBc
        meta.Campos[9].Definidor(registro, "300,00".AsSpan());       // VlIpi
        meta.Campos[10].Definidor(registro, "OBS001".AsSpan());      // CodObs

        registro.CstIcms.Should().Be(60);
        registro.Cfop.Should().Be(Cfop.Create("5201".AsSpan()));
        registro.AliqIcms.Should().Be(12.00m);
        registro.VlOpr.Should().Be(15000.00m);
        registro.VlBcIcms.Should().Be(10000.00m);
        registro.VlIcms.Should().Be(1200.00m);
        registro.VlBcIcmsSt.Should().Be(5000.00m);
        registro.VlIcmsSt.Should().Be(600.00m);
        registro.VlRedBc.Should().Be(0.00m);
        registro.VlIpi.Should().Be(300.00m);
        registro.CodObs.Should().Be("OBS001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C190".AsSpan(), out var meta);
        var registro = (RegistroC190)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CstIcms.Should().BeNull();
        registro.Cfop.Should().BeNull();
        registro.AliqIcms.Should().BeNull();
        registro.VlOpr.Should().BeNull();
        registro.VlBcIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
        registro.VlBcIcmsSt.Should().BeNull();
        registro.VlIcmsSt.Should().BeNull();
        registro.VlRedBc.Should().BeNull();
        registro.VlIpi.Should().BeNull();
        registro.CodObs.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // CST_ICMS int? serializa sem zero-padding — forma canônica é "60" não "060".
        const string sped =
            "|C190|60|5201|12,00|15000,00|10000,00|1200,00|5000,00|600,00|0,00|300,00|OBS001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComAliquotaVazia_PreservaTextoCanonico()
    {
        // CST 041 (isento): ALIQ_ICMS vazio, demais valores zerados. CST "41" sem zero-padding.
        const string sped =
            "|C190|41|5102||10000,00|0,00|0,00|0,00|0,00|0,00|0,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComObservacaoVazia_PreservaTextoCanonico()
    {
        // CST 000: int? serializa como "0". COD_OBS campo 12 vazio.
        const string sped =
            "|C190|0|5102|12,00|5000,00|5000,00|600,00|0,00|0,00|0,00|0,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

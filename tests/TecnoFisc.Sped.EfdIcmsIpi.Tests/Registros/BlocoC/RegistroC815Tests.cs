using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.107 — exercita a forma do <see cref="RegistroC815"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 153): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC815Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC815).Assembly);

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
    public void Atributo_DeclaraC815_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC815).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C815");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC815Com13CamposNaOrdem()
    {
        _catalogo.TentarObter("C815".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C815");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodMotRestCompl", "QuantConv", "Unid", "VlUnitConv",
            "VlUnitIcmsNaOperacaoConv", "VlUnitIcmsOpConv", "VlUnitIcmsOpEstoqueConv",
            "VlUnitIcmsStEstoqueConv", "VlUnitFcpIcmsStEstoqueConv",
            "VlUnitIcmsStConvRest", "VlUnitFcpStConvRest",
            "VlUnitIcmsStConvCompl", "VlUnitFcpStConvCompl"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C815".AsSpan(), out var meta);
        var registro = (RegistroC815)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "12300".AsSpan());          // CodMotRestCompl
        meta.Campos[1].Definidor(registro, "2,000000".AsSpan());       // QuantConv
        meta.Campos[2].Definidor(registro, "UN".AsSpan());             // Unid
        meta.Campos[3].Definidor(registro, "10,500000".AsSpan());      // VlUnitConv
        meta.Campos[4].Definidor(registro, "1,000000".AsSpan());       // VlUnitIcmsNaOperacaoConv
        meta.Campos[5].Definidor(registro, "0,800000".AsSpan());       // VlUnitIcmsOpConv
        meta.Campos[6].Definidor(registro, "0,900000".AsSpan());       // VlUnitIcmsOpEstoqueConv
        meta.Campos[7].Definidor(registro, "1,200000".AsSpan());       // VlUnitIcmsStEstoqueConv
        meta.Campos[8].Definidor(registro, "0,100000".AsSpan());       // VlUnitFcpIcmsStEstoqueConv
        meta.Campos[9].Definidor(registro, "0,700000".AsSpan());       // VlUnitIcmsStConvRest
        meta.Campos[10].Definidor(registro, "0,050000".AsSpan());      // VlUnitFcpStConvRest
        meta.Campos[11].Definidor(registro, "0,300000".AsSpan());      // VlUnitIcmsStConvCompl
        meta.Campos[12].Definidor(registro, "0,020000".AsSpan());      // VlUnitFcpStConvCompl

        registro.CodMotRestCompl.Should().Be("12300");
        registro.QuantConv.Should().Be(2.000000m);
        registro.Unid.Should().Be("UN");
        registro.VlUnitConv.Should().Be(10.500000m);
        registro.VlUnitIcmsNaOperacaoConv.Should().Be(1.000000m);
        registro.VlUnitIcmsOpConv.Should().Be(0.800000m);
        registro.VlUnitIcmsOpEstoqueConv.Should().Be(0.900000m);
        registro.VlUnitIcmsStEstoqueConv.Should().Be(1.200000m);
        registro.VlUnitFcpIcmsStEstoqueConv.Should().Be(0.100000m);
        registro.VlUnitIcmsStConvRest.Should().Be(0.700000m);
        registro.VlUnitFcpStConvRest.Should().Be(0.050000m);
        registro.VlUnitIcmsStConvCompl.Should().Be(0.300000m);
        registro.VlUnitFcpStConvCompl.Should().Be(0.020000m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C815".AsSpan(), out var meta);
        var registro = (RegistroC815)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodMotRestCompl.Should().BeNull();
        registro.QuantConv.Should().BeNull();
        registro.Unid.Should().BeNull();
        registro.VlUnitConv.Should().BeNull();
        registro.VlUnitIcmsNaOperacaoConv.Should().BeNull();
        registro.VlUnitIcmsOpConv.Should().BeNull();
        registro.VlUnitIcmsOpEstoqueConv.Should().BeNull();
        registro.VlUnitIcmsStEstoqueConv.Should().BeNull();
        registro.VlUnitFcpIcmsStEstoqueConv.Should().BeNull();
        registro.VlUnitIcmsStConvRest.Should().BeNull();
        registro.VlUnitFcpStConvRest.Should().BeNull();
        registro.VlUnitIcmsStConvCompl.Should().BeNull();
        registro.VlUnitFcpStConvCompl.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C815|12300|2,000000|UN|10,500000|1,000000|0,800000|0,900000|1,200000|0,100000|0,700000|0,050000|0,300000|0,020000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // Campos 06-14 (OC) omitidos — apenas COD_MOT_REST_COMPL, QUANT_CONV, UNID e VL_UNIT_CONV preenchidos.
        const string sped =
            "|C815|12300|2,000000|UN|10,500000||||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

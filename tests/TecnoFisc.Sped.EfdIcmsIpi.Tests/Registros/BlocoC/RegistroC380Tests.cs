using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.078 — exercita a forma do <see cref="RegistroC380"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 114): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC380Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC380).Assembly);

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
    public void Atributo_DeclaraC380_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC380).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C380");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC380Com15CamposNaOrdem()
    {
        _catalogo.TentarObter("C380".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C380");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodMotRestCompl",
            "QuantConv",
            "Unid",
            "VlUnitConv",
            "VlUnitIcmsNaOperacaoConv",
            "VlUnitIcmsOpConv",
            "VlUnitIcmsOpEstoqueConv",
            "VlUnitIcmsStEstoqueConv",
            "VlUnitFcpIcmsStEstoqueConv",
            "VlUnitIcmsStConvRest",
            "VlUnitFcpStConvRest",
            "VlUnitIcmsStConvCompl",
            "VlUnitFcpStConvCompl",
            "CstIcms",
            "Cfop",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C380".AsSpan(), out var meta);
        var registro = (RegistroC380)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "00100".AsSpan());          // CodMotRestCompl
        meta.Campos[1].Definidor(registro, "10,000000".AsSpan());      // QuantConv
        meta.Campos[2].Definidor(registro, "UN".AsSpan());             // Unid
        meta.Campos[3].Definidor(registro, "50,000000".AsSpan());      // VlUnitConv
        meta.Campos[4].Definidor(registro, "9,000000".AsSpan());       // VlUnitIcmsNaOperacaoConv
        meta.Campos[5].Definidor(registro, "9,000000".AsSpan());       // VlUnitIcmsOpConv
        meta.Campos[6].Definidor(registro, "8,500000".AsSpan());       // VlUnitIcmsOpEstoqueConv
        meta.Campos[7].Definidor(registro, "12,000000".AsSpan());      // VlUnitIcmsStEstoqueConv
        meta.Campos[8].Definidor(registro, "1,200000".AsSpan());       // VlUnitFcpIcmsStEstoqueConv
        meta.Campos[9].Definidor(registro, "3,500000".AsSpan());       // VlUnitIcmsStConvRest
        meta.Campos[10].Definidor(registro, "0,350000".AsSpan());      // VlUnitFcpStConvRest
        meta.Campos[11].Definidor(registro, "0,000000".AsSpan());      // VlUnitIcmsStConvCompl
        meta.Campos[12].Definidor(registro, "0,000000".AsSpan());      // VlUnitFcpStConvCompl
        meta.Campos[13].Definidor(registro, "010".AsSpan());           // CstIcms
        meta.Campos[14].Definidor(registro, "5102".AsSpan());          // Cfop

        registro.CodMotRestCompl.Should().Be("00100");
        registro.QuantConv.Should().Be(10.000000m);
        registro.Unid.Should().Be("UN");
        registro.VlUnitConv.Should().Be(50.000000m);
        registro.VlUnitIcmsNaOperacaoConv.Should().Be(9.000000m);
        registro.VlUnitIcmsOpConv.Should().Be(9.000000m);
        registro.VlUnitIcmsOpEstoqueConv.Should().Be(8.500000m);
        registro.VlUnitIcmsStEstoqueConv.Should().Be(12.000000m);
        registro.VlUnitFcpIcmsStEstoqueConv.Should().Be(1.200000m);
        registro.VlUnitIcmsStConvRest.Should().Be(3.500000m);
        registro.VlUnitFcpStConvRest.Should().Be(0.350000m);
        registro.VlUnitIcmsStConvCompl.Should().Be(0.000000m);
        registro.VlUnitFcpStConvCompl.Should().Be(0.000000m);
        registro.CstIcms.Should().Be(10);
        registro.Cfop.Should().Be(Cfop.Criar("5102".AsSpan()));
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C380".AsSpan(), out var meta);
        var registro = (RegistroC380)meta!.Fabrica();

        foreach (var campo in meta.Campos)
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
        registro.CstIcms.Should().BeNull();
        registro.Cfop.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C380|00100|10,000000|UN|50,000000|9,000000|9,000000|8,500000|12,000000|1,200000|3,500000|0,350000|0,000000|0,000000|10|5102|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // Campos 06-14 (VL_UNIT_ICMS_NA_OPERACAO_CONV … VL_UNIT_FCP_ST_CONV_COMPL) são OC — 9 campos vazios = 10 pipes.
        const string sped =
            "|C380|00200|5,000000|KG|25,000000||||||||||30|5405|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

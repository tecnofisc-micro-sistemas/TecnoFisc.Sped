using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.085 — exercita a forma do <see cref="RegistroC430"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 122): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC430Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC430).Assembly);

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
    public void Atributo_DeclaraC430_Nivel6_BlocoC()
    {
        var atributo = typeof(RegistroC430).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C430");
        atributo.Nivel.Should().Be(6);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC430Com15CamposNaOrdem()
    {
        _catalogo.TentarObter("C430".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C430");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodMotRestCompl", "QuantConv", "Unid", "VlUnitConv",
            "VlUnitIcmsNaOperacaoConv", "VlUnitIcmsOpConv", "VlUnitIcmsOpEstoqueConv",
            "VlUnitIcmsStEstoqueConv", "VlUnitFcpIcmsStEstoqueConv",
            "VlUnitIcmsStConvRest", "VlUnitFcpStConvRest",
            "VlUnitIcmsStConvCompl", "VlUnitFcpStConvCompl",
            "CstIcms", "Cfop"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C430".AsSpan(), out var meta);
        var registro = (RegistroC430)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "00507".AsSpan());       // CodMotRestCompl
        meta.Campos[1].Definidor(registro, "10,000000".AsSpan());   // QuantConv
        meta.Campos[2].Definidor(registro, "UN".AsSpan());          // Unid
        meta.Campos[3].Definidor(registro, "50,000000".AsSpan());   // VlUnitConv
        meta.Campos[4].Definidor(registro, "4,500000".AsSpan());    // VlUnitIcmsNaOperacaoConv
        meta.Campos[5].Definidor(registro, "5,000000".AsSpan());    // VlUnitIcmsOpConv
        meta.Campos[6].Definidor(registro, "5,000000".AsSpan());    // VlUnitIcmsOpEstoqueConv
        meta.Campos[7].Definidor(registro, "7,200000".AsSpan());    // VlUnitIcmsStEstoqueConv
        meta.Campos[8].Definidor(registro, "0,360000".AsSpan());    // VlUnitFcpIcmsStEstoqueConv
        meta.Campos[9].Definidor(registro, "6,840000".AsSpan());    // VlUnitIcmsStConvRest
        meta.Campos[10].Definidor(registro, "0,360000".AsSpan());   // VlUnitFcpStConvRest
        meta.Campos[11].Definidor(registro, "0,000000".AsSpan());   // VlUnitIcmsStConvCompl
        meta.Campos[12].Definidor(registro, "0,000000".AsSpan());   // VlUnitFcpStConvCompl
        meta.Campos[13].Definidor(registro, "060".AsSpan());        // CstIcms
        meta.Campos[14].Definidor(registro, "5102".AsSpan());       // Cfop

        registro.CodMotRestCompl.Should().Be("00507");
        registro.QuantConv.Should().Be(10.000000m);
        registro.Unid.Should().Be("UN");
        registro.VlUnitConv.Should().Be(50.000000m);
        registro.VlUnitIcmsNaOperacaoConv.Should().Be(4.500000m);
        registro.VlUnitIcmsOpConv.Should().Be(5.000000m);
        registro.VlUnitIcmsOpEstoqueConv.Should().Be(5.000000m);
        registro.VlUnitIcmsStEstoqueConv.Should().Be(7.200000m);
        registro.VlUnitFcpIcmsStEstoqueConv.Should().Be(0.360000m);
        registro.VlUnitIcmsStConvRest.Should().Be(6.840000m);
        registro.VlUnitFcpStConvRest.Should().Be(0.360000m);
        registro.VlUnitIcmsStConvCompl.Should().Be(0.000000m);
        registro.VlUnitFcpStConvCompl.Should().Be(0.000000m);
        registro.CstIcms.Should().Be(60);
        registro.Cfop.Should().Be(Cfop.Create("5102".AsSpan()));
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C430".AsSpan(), out var meta);
        var registro = (RegistroC430)meta!.Fabrica();

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
        // CST_ICMS int? serializa sem zero-padding — forma canônica é "60" não "060".
        const string sped =
            "|C430|00507|10,000000|UN|50,000000|4,500000|5,000000|5,000000|7,200000|0,360000|6,840000|0,360000|0,000000|0,000000|60|5102|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SomenteObrigatorios_PreservaTextoCanonico()
    {
        // Apenas campos obrigatórios; campos OC (06 a 14) vazios. CST "60" sem zero-padding.
        const string sped =
            "|C430|00608|2,500000|PC|30,000000||||||||||60|5405|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.063 — exercita a forma do <see cref="RegistroC180"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 91): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC180Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC180).Assembly);

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
    public void Atributo_DeclaraC180_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC180).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C180");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC180Com10CamposNaOrdem()
    {
        _catalogo.TentarObter("C180".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C180");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodRespRet", "QuantConv", "Unid",
            "VlUnitConv", "VlUnitIcmsOpConv", "VlUnitBcIcmsStConv", "VlUnitIcmsStConv",
            "VlUnitFcpStConv", "CodDa", "NumDa"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C180".AsSpan(), out var meta);
        var registro = (RegistroC180)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());             // CodRespRet
        meta.Campos[1].Definidor(registro, "10,000000".AsSpan());     // QuantConv
        meta.Campos[2].Definidor(registro, "UN".AsSpan());            // Unid
        meta.Campos[3].Definidor(registro, "50,000000".AsSpan());     // VlUnitConv
        meta.Campos[4].Definidor(registro, "5,000000".AsSpan());      // VlUnitIcmsOpConv
        meta.Campos[5].Definidor(registro, "60,000000".AsSpan());     // VlUnitBcIcmsStConv
        meta.Campos[6].Definidor(registro, "7,200000".AsSpan());      // VlUnitIcmsStConv
        meta.Campos[7].Definidor(registro, "0,500000".AsSpan());      // VlUnitFcpStConv
        meta.Campos[8].Definidor(registro, "1".AsSpan());             // CodDa
        meta.Campos[9].Definidor(registro, "12345".AsSpan());         // NumDa

        registro.CodRespRet.Should().Be(CodigoResponsavelRetencaoSt.RemetenteDireto);
        registro.QuantConv.Should().Be(10.000000m);
        registro.Unid.Should().Be("UN");
        registro.VlUnitConv.Should().Be(50.000000m);
        registro.VlUnitIcmsOpConv.Should().Be(5.000000m);
        registro.VlUnitBcIcmsStConv.Should().Be(60.000000m);
        registro.VlUnitIcmsStConv.Should().Be(7.200000m);
        registro.VlUnitFcpStConv.Should().Be(0.500000m);
        registro.CodDa.Should().Be(CodigoModeloDocumentoArrecadacao.Gnre);
        registro.NumDa.Should().Be("12345");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C180".AsSpan(), out var meta);
        var registro = (RegistroC180)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodRespRet.Should().BeNull();
        registro.QuantConv.Should().BeNull();
        registro.Unid.Should().BeNull();
        registro.VlUnitConv.Should().BeNull();
        registro.VlUnitIcmsOpConv.Should().BeNull();
        registro.VlUnitBcIcmsStConv.Should().BeNull();
        registro.VlUnitIcmsStConv.Should().BeNull();
        registro.VlUnitFcpStConv.Should().BeNull();
        registro.CodDa.Should().BeNull();
        registro.NumDa.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C180|1|10,000000|UN|50,000000|5,000000|60,000000|7,200000|0,500000|1|12345|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // FCP ST, COD_DA e NUM_DA opcionais; entrada sem GNRE e sem FCP.
        const string sped = "|C180|2|5,000000|PC|30,000000|3,600000|36,000000|4,320000||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ProprioDeclarante_ComGnre_PreservaTextoCanonico()
    {
        // Próprio declarante (COD_RESP_RET=3) com GNRE preenchida.
        const string sped = "|C180|3|2,500000|CX|15,000000|1,800000|18,000000|2,160000||0|GNRE-2024-001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

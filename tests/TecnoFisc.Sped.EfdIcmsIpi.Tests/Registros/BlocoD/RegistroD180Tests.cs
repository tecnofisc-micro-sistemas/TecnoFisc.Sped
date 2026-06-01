using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.126 — exercita a forma do <see cref="RegistroD180"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 176-177): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD180Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD180).Assembly);

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
    public void Atributo_DeclaraD180_Nivel3_BlocoD()
    {
        var atributo = typeof(RegistroD180).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D180");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD180Com16CamposNaOrdem()
    {
        _catalogo.TentarObter("D180".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D180");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "NumSeq", "IndEmit", "CnpjCpfEmit", "UfEmit", "IeEmit",
            "CodMunOrig", "CnpjCpfTom", "UfTom", "IeTom", "CodMunDest",
            "CodMod", "Ser", "Sub", "NumDoc", "DtDoc", "VlDoc",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 16));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D180".AsSpan(), out var meta);
        var registro = (RegistroD180)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());               // NumSeq
        meta.Campos[1].Definidor(registro, "0".AsSpan());               // IndEmit
        meta.Campos[2].Definidor(registro, "12345678000195".AsSpan());  // CnpjCpfEmit
        meta.Campos[3].Definidor(registro, "SP".AsSpan());              // UfEmit
        meta.Campos[4].Definidor(registro, "123456789".AsSpan());       // IeEmit
        meta.Campos[5].Definidor(registro, "3550308".AsSpan());         // CodMunOrig
        meta.Campos[6].Definidor(registro, "98765432000177".AsSpan());  // CnpjCpfTom
        meta.Campos[7].Definidor(registro, "RJ".AsSpan());              // UfTom
        meta.Campos[8].Definidor(registro, "987654321".AsSpan());       // IeTom
        meta.Campos[9].Definidor(registro, "3304557".AsSpan());         // CodMunDest
        meta.Campos[10].Definidor(registro, "26".AsSpan());             // CodMod
        meta.Campos[11].Definidor(registro, "001".AsSpan());            // Ser
        meta.Campos[12].Definidor(registro, "1".AsSpan());              // Sub
        meta.Campos[13].Definidor(registro, "100001".AsSpan());         // NumDoc
        meta.Campos[14].Definidor(registro, "01012020".AsSpan());       // DtDoc
        meta.Campos[15].Definidor(registro, "15000,00".AsSpan());       // VlDoc

        registro.NumSeq.Should().Be(1);
        registro.IndEmit.Should().Be(IndicadorEmissorDocumento.EmissaoPropria);
        registro.CnpjCpfEmit.Should().Be("12345678000195");
        registro.UfEmit.Should().Be("SP");
        registro.IeEmit.Should().Be("123456789");
        registro.CodMunOrig.Should().Be(3550308);
        registro.CnpjCpfTom.Should().Be("98765432000177");
        registro.UfTom.Should().Be("RJ");
        registro.IeTom.Should().Be("987654321");
        registro.CodMunDest.Should().Be(3304557);
        registro.CodMod.Should().Be(ModeloDocumento.Create("26"));
        registro.Ser.Should().Be("001");
        registro.Sub.Should().Be("1");
        registro.NumDoc.Should().Be(100001);
        registro.DtDoc.Should().Be(new DateOnly(2020, 1, 1));
        registro.VlDoc.Should().Be(15000.00m);
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("D180".AsSpan(), out var meta);
        var registro = (RegistroD180)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.NumSeq.Should().BeNull();
        registro.IndEmit.Should().BeNull();
        registro.CnpjCpfEmit.Should().BeNull();
        registro.UfEmit.Should().BeNull();
        registro.IeEmit.Should().BeNull();
        registro.CodMunOrig.Should().BeNull();
        registro.CnpjCpfTom.Should().BeNull();
        registro.UfTom.Should().BeNull();
        registro.IeTom.Should().BeNull();
        registro.CodMunDest.Should().BeNull();
        registro.CodMod.Should().BeNull();
        registro.Ser.Should().BeNull();
        registro.Sub.Should().BeNull();
        registro.NumDoc.Should().BeNull();
        registro.DtDoc.Should().BeNull();
        registro.VlDoc.Should().BeNull();
    }

    [Theory]
    [InlineData("0", IndicadorEmissorDocumento.EmissaoPropria)]
    [InlineData("1", IndicadorEmissorDocumento.Terceiros)]
    public void Definidor_IndEmit_ConverteLiteral(string literal, IndicadorEmissorDocumento esperado)
    {
        _catalogo.TentarObter("D180".AsSpan(), out var meta);
        var registro = (RegistroD180)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, literal.AsSpan());

        registro.IndEmit.Should().Be(esperado);
    }

    [Fact]
    public void Definidor_IndEmit_Vazio_DevolveNulo()
    {
        _catalogo.TentarObter("D180".AsSpan(), out var meta);
        var registro = (RegistroD180)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, Span<char>.Empty);

        registro.IndEmit.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Modal rodoviário com emitente SP, tomador RJ, CT de Transporte Multimodal (cód. 26).
        const string sped = "|D180|1|0|12345678000195|SP|123456789|3550308|98765432000177|RJ|987654321|3304557|26|001|1|100001|01012020|15000,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // Campos opcionais IeEmit, IeTom e Sub vazios.
        const string sped = "|D180|2|1|98765432000177|MG||3106200|12345678000195|SP||3550308|08|B|001||15122023|8500,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComExterior_PreservaTextoCanonico()
    {
        // Destino exterior (município 9999999) com aéreo (cód. 10).
        const string sped = "|D180|3|0|11111111000191|RS|RS123456|3301702|22222222000180|RS|RS654321|9999999|10|001||300001|20062022|25000,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

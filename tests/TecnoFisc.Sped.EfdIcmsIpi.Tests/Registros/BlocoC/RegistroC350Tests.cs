using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.076 — exercita a forma do <see cref="RegistroC350"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 113): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC350Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC350).Assembly);

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
    public void Atributo_DeclaraC350_Nivel2_BlocoC()
    {
        var atributo = typeof(RegistroC350).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C350");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC350Com11CamposNaOrdem()
    {
        _catalogo.TentarObter("C350".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C350");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "Ser",
            "SubSer",
            "NumDoc",
            "DtDoc",
            "CnpjCpf",
            "VlMerc",
            "VlDoc",
            "VlDesc",
            "VlPis",
            "VlCofins",
            "CodCta",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C350".AsSpan(), out var meta);
        var registro = (RegistroC350)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "001".AsSpan());              // Ser
        meta.Campos[1].Definidor(registro, "01".AsSpan());               // SubSer
        meta.Campos[2].Definidor(registro, "12345".AsSpan());            // NumDoc
        meta.Campos[3].Definidor(registro, "01032024".AsSpan());         // DtDoc
        meta.Campos[4].Definidor(registro, "12345678000195".AsSpan());   // CnpjCpf
        meta.Campos[5].Definidor(registro, "1500,00".AsSpan());          // VlMerc
        meta.Campos[6].Definidor(registro, "1500,00".AsSpan());          // VlDoc
        meta.Campos[7].Definidor(registro, "50,00".AsSpan());            // VlDesc
        meta.Campos[8].Definidor(registro, "5,00".AsSpan());             // VlPis
        meta.Campos[9].Definidor(registro, "23,00".AsSpan());            // VlCofins
        meta.Campos[10].Definidor(registro, "3.01.01".AsSpan());         // CodCta

        registro.Ser.Should().Be("001");
        registro.SubSer.Should().Be("01");
        registro.NumDoc.Should().Be(12345);
        registro.DtDoc.Should().Be(new DateOnly(2024, 3, 1));
        registro.CnpjCpf.Should().Be("12345678000195");
        registro.VlMerc.Should().Be(1500m);
        registro.VlDoc.Should().Be(1500m);
        registro.VlDesc.Should().Be(50m);
        registro.VlPis.Should().Be(5m);
        registro.VlCofins.Should().Be(23m);
        registro.CodCta.Should().Be("3.01.01");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C350".AsSpan(), out var meta);
        var registro = (RegistroC350)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty); // Ser
        meta.Campos[1].Definidor(registro, Span<char>.Empty); // SubSer
        meta.Campos[2].Definidor(registro, Span<char>.Empty); // NumDoc
        meta.Campos[4].Definidor(registro, Span<char>.Empty); // CnpjCpf
        meta.Campos[7].Definidor(registro, Span<char>.Empty); // VlDesc
        meta.Campos[8].Definidor(registro, Span<char>.Empty); // VlPis
        meta.Campos[9].Definidor(registro, Span<char>.Empty); // VlCofins
        meta.Campos[10].Definidor(registro, Span<char>.Empty); // CodCta

        registro.Ser.Should().BeNull();
        registro.SubSer.Should().BeNull();
        registro.NumDoc.Should().BeNull();
        registro.CnpjCpf.Should().BeNull();
        registro.VlDesc.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C350|001|01|12345|01032024|12345678000195|1500,00|1500,00|50,00|5,00|23,00|3.01.01|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SomenteObrigatorios_PreservaTextoCanonico()
    {
        // SER, SUB_SER, CNPJ_CPF, VL_DESC, VL_PIS, VL_COFINS e COD_CTA são opcionais.
        const string sped =
            "|C350|||1|01012023||500,00|500,00|||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

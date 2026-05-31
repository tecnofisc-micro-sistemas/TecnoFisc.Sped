using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.139 — exercita a forma do <see cref="RegistroD400"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 188): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD400Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD400).Assembly);

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
    public void Atributo_DeclaraD400_Nivel2_BlocoD()
    {
        var atributo = typeof(RegistroD400).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D400");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD400Com15CamposNaOrdem()
    {
        _catalogo.TentarObter("D400".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D400");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["CodPart", "CodMod", "CodSit", "Ser", "Sub", "NumDoc", "DtDoc",
             "VlDoc", "VlDesc", "VlServ", "VlBcIcms", "VlIcms", "VlPis", "VlCofins", "CodCta"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D400".AsSpan(), out var meta);
        var registro = (RegistroD400)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "AGENCIA001".AsSpan());   // CodPart
        meta.Campos[1].Definidor(registro, "18".AsSpan());            // CodMod
        meta.Campos[2].Definidor(registro, "00".AsSpan());            // CodSit
        meta.Campos[3].Definidor(registro, "SER1".AsSpan());          // Ser
        meta.Campos[4].Definidor(registro, "1".AsSpan());             // Sub
        meta.Campos[5].Definidor(registro, "12345".AsSpan());         // NumDoc
        meta.Campos[6].Definidor(registro, "01012024".AsSpan());      // DtDoc
        meta.Campos[7].Definidor(registro, "1500,00".AsSpan());       // VlDoc
        meta.Campos[8].Definidor(registro, "50,00".AsSpan());         // VlDesc
        meta.Campos[9].Definidor(registro, "1450,00".AsSpan());       // VlServ
        meta.Campos[10].Definidor(registro, "1200,00".AsSpan());      // VlBcIcms
        meta.Campos[11].Definidor(registro, "216,00".AsSpan());       // VlIcms
        meta.Campos[12].Definidor(registro, "10,00".AsSpan());        // VlPis
        meta.Campos[13].Definidor(registro, "46,00".AsSpan());        // VlCofins
        meta.Campos[14].Definidor(registro, "CAIXA-01".AsSpan());     // CodCta

        registro.CodPart.Should().Be("AGENCIA001");
        registro.CodMod.Should().Be("18");
        registro.CodSit.Should().Be(CodigoSituacaoDocumentoFiscal.DocumentoRegular);
        registro.Ser.Should().Be("SER1");
        registro.Sub.Should().Be(1);
        registro.NumDoc.Should().Be(12345);
        registro.DtDoc.Should().Be(new DateOnly(2024, 1, 1));
        registro.VlDoc.Should().Be(1500.00m);
        registro.VlDesc.Should().Be(50.00m);
        registro.VlServ.Should().Be(1450.00m);
        registro.VlBcIcms.Should().Be(1200.00m);
        registro.VlIcms.Should().Be(216.00m);
        registro.VlPis.Should().Be(10.00m);
        registro.VlCofins.Should().Be(46.00m);
        registro.CodCta.Should().Be("CAIXA-01");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D400".AsSpan(), out var meta);
        var registro = (RegistroD400)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, Span<char>.Empty);   // Ser
        meta.Campos[4].Definidor(registro, Span<char>.Empty);   // Sub
        meta.Campos[8].Definidor(registro, Span<char>.Empty);   // VlDesc
        meta.Campos[10].Definidor(registro, Span<char>.Empty);  // VlBcIcms
        meta.Campos[11].Definidor(registro, Span<char>.Empty);  // VlIcms
        meta.Campos[12].Definidor(registro, Span<char>.Empty);  // VlPis
        meta.Campos[13].Definidor(registro, Span<char>.Empty);  // VlCofins
        meta.Campos[14].Definidor(registro, Span<char>.Empty);  // CodCta

        registro.Ser.Should().BeNull();
        registro.Sub.Should().BeNull();
        registro.VlDesc.Should().BeNull();
        registro.VlBcIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|D400|AGENCIA001|18|00|SER1|1|12345|01012024|1500,00|50,00|1450,00|1200,00|216,00|10,00|46,00|CAIXA-01|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // RMD regular com campos opcionais ausentes (SER, SUB, VL_DESC, VL_BC_ICMS, VL_ICMS, VL_PIS, VL_COFINS, COD_CTA).
        const string sped =
            "|D400|AGENCIA001|18|00|||12345|01012024|1500,00||1450,00||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_DocumentoCancelado_PreservaTextoCanonico()
    {
        // Exceção 1: COD_SIT "02" (cancelado) — campos de valor devem ser VAZIO conforme guia.
        const string sped =
            "|D400|AGENCIA001|18|02|SER1||12345|01012024|0,00||0,00||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.105 — exercita a forma do <see cref="RegistroC800"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 151): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC800Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC800).Assembly);

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
    public void Atributo_DeclaraC800_Nivel2_BlocoC()
    {
        var atributo = typeof(RegistroC800).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C800");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC800Com16CamposNaOrdem()
    {
        _catalogo.TentarObter("C800".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C800");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodMod", "CodSit", "NumCfe", "DtDoc",
            "VlCfe", "VlPis", "VlCofins", "CnpjCpf",
            "NrSat", "ChvCfe", "VlDesc", "VlMerc",
            "VlOutDa", "VlIcms", "VlPisSt", "VlCofinsSt"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C800".AsSpan(), out var meta);
        var registro = (RegistroC800)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "59".AsSpan());                                               // CodMod
        meta.Campos[1].Definidor(registro, "00".AsSpan());                                               // CodSit
        meta.Campos[2].Definidor(registro, "1".AsSpan());                                                // NumCfe
        meta.Campos[3].Definidor(registro, "01012024".AsSpan());                                         // DtDoc
        meta.Campos[4].Definidor(registro, "100,00".AsSpan());                                           // VlCfe
        meta.Campos[5].Definidor(registro, "5,00".AsSpan());                                             // VlPis
        meta.Campos[6].Definidor(registro, "10,00".AsSpan());                                            // VlCofins
        meta.Campos[7].Definidor(registro, "12345678901234".AsSpan());                                   // CnpjCpf
        meta.Campos[8].Definidor(registro, "900000001".AsSpan());                                        // NrSat
        meta.Campos[9].Definidor(registro, "12345678901234567890123456789012345678901234".AsSpan());     // ChvCfe
        meta.Campos[10].Definidor(registro, "5,00".AsSpan());                                            // VlDesc
        meta.Campos[11].Definidor(registro, "90,00".AsSpan());                                           // VlMerc
        meta.Campos[12].Definidor(registro, "0,00".AsSpan());                                            // VlOutDa
        meta.Campos[13].Definidor(registro, "18,00".AsSpan());                                           // VlIcms
        meta.Campos[14].Definidor(registro, "0,00".AsSpan());                                            // VlPisSt
        meta.Campos[15].Definidor(registro, "0,00".AsSpan());                                            // VlCofinsSt

        registro.CodMod.Should().Be("59");
        registro.CodSit.Should().Be(CodigoSituacaoDocumentoFiscal.DocumentoRegular);
        registro.NumCfe.Should().Be(1);
        registro.DtDoc.Should().Be(new DateOnly(2024, 1, 1));
        registro.VlCfe.Should().Be(100.00m);
        registro.VlPis.Should().Be(5.00m);
        registro.VlCofins.Should().Be(10.00m);
        registro.CnpjCpf.Should().Be("12345678901234");
        registro.NrSat.Should().Be(900000001);
        registro.ChvCfe.Should().Be("12345678901234567890123456789012345678901234");
        registro.VlDesc.Should().Be(5.00m);
        registro.VlMerc.Should().Be(90.00m);
        registro.VlOutDa.Should().Be(0.00m);
        registro.VlIcms.Should().Be(18.00m);
        registro.VlPisSt.Should().Be(0.00m);
        registro.VlCofinsSt.Should().Be(0.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C800".AsSpan(), out var meta);
        var registro = (RegistroC800)meta!.Fabrica();

        // Índices 1 (CodSit), 2 (NumCfe), 3 (DtDoc), 4 (VlCfe), 8 (NrSat), 10–15 são não-nullable — não testados aqui.
        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodMod
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlPis
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlCofins
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);  // CnpjCpf
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);  // ChvCfe

        registro.CodMod.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CnpjCpf.Should().BeNull();
        registro.ChvCfe.Should().BeNull();
    }

    [Theory]
    [InlineData(CodigoSituacaoDocumentoFiscal.DocumentoRegular, "00")]
    [InlineData(CodigoSituacaoDocumentoFiscal.DocumentoRegularExtemporaneo, "01")]
    [InlineData(CodigoSituacaoDocumentoFiscal.DocumentoCancelado, "02")]
    [InlineData(CodigoSituacaoDocumentoFiscal.DocumentoCanceladoExtemporaneo, "03")]
    public void Serializar_CodSit_RetornaCodigo(CodigoSituacaoDocumentoFiscal situacao, string esperado)
    {
        _catalogo.TentarObter("C800".AsSpan(), out var meta);
        var registro = (RegistroC800)meta!.Fabrica();
        registro.CodSit = situacao;

        meta.Campos[1].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C800|59|00|1|01012024|100,00|5,00|10,00|12345678901234|900000001|12345678901234567890123456789012345678901234|5,00|90,00|0,00|18,00|0,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // VL_PIS (07) e VL_COFINS (08) dispensados para contribuintes que entregam EFD-Contribuições.
        const string sped =
            "|C800|59|00|42|15032024|89,90|||12345678000195|900000001|12345678901234567890123456789012345678901234|2,00|87,90|0,00|16,20|0,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

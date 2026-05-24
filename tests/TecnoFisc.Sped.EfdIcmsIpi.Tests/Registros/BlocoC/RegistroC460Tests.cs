using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.086 — exercita a forma do <see cref="RegistroC460"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 125): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC460Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC460).Assembly);

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
    public void Atributo_DeclaraC460_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC460).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C460");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC460Com9CamposNaOrdem()
    {
        _catalogo.TentarObter("C460".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C460");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodMod", "CodSit", "NumDoc", "DtDoc",
            "VlDoc", "VlPis", "VlCofins", "CpfCnpj", "NomAdq"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C460".AsSpan(), out var meta);
        var registro = (RegistroC460)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "02".AsSpan());              // CodMod
        meta.Campos[1].Definidor(registro, "00".AsSpan());              // CodSit
        meta.Campos[2].Definidor(registro, "1234".AsSpan());            // NumDoc
        meta.Campos[3].Definidor(registro, "01012023".AsSpan());        // DtDoc
        meta.Campos[4].Definidor(registro, "150,00".AsSpan());          // VlDoc
        meta.Campos[5].Definidor(registro, "2,47".AsSpan());            // VlPis
        meta.Campos[6].Definidor(registro, "11,40".AsSpan());           // VlCofins
        meta.Campos[7].Definidor(registro, "12345678901234".AsSpan());  // CpfCnpj
        meta.Campos[8].Definidor(registro, "JOSE DA SILVA".AsSpan());   // NomAdq

        registro.CodMod.Should().Be("02");
        registro.CodSit.Should().Be(CodigoSituacaoDocumentoFiscal.DocumentoRegular);
        registro.NumDoc.Should().Be(1234);
        registro.DtDoc.Should().Be(new DateOnly(2023, 1, 1));
        registro.VlDoc.Should().Be(150.00m);
        registro.VlPis.Should().Be(2.47m);
        registro.VlCofins.Should().Be(11.40m);
        registro.CpfCnpj.Should().Be("12345678901234");
        registro.NomAdq.Should().Be("JOSE DA SILVA");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C460".AsSpan(), out var meta);
        var registro = (RegistroC460)meta!.Fabrica();

        // CodSit (índice 1), DtDoc (3) e VlDoc (4) são não-nullable — não testados aqui.
        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty); // CodMod
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // NumDoc
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // VlPis
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCofins
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty); // CpfCnpj
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty); // NomAdq

        registro.CodMod.Should().BeNull();
        registro.NumDoc.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CpfCnpj.Should().BeNull();
        registro.NomAdq.Should().BeNull();
    }

    [Theory]
    [InlineData(CodigoSituacaoDocumentoFiscal.DocumentoRegular, "00")]
    [InlineData(CodigoSituacaoDocumentoFiscal.DocumentoRegularExtemporaneo, "01")]
    [InlineData(CodigoSituacaoDocumentoFiscal.DocumentoCancelado, "02")]
    public void Serializar_CodSit_RetornaCodigo(CodigoSituacaoDocumentoFiscal situacao, string esperado)
    {
        _catalogo.TentarObter("C460".AsSpan(), out var meta);
        var registro = (RegistroC460)meta!.Fabrica();
        registro.CodSit = situacao;

        meta.Campos[1].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C460|02|00|1234|01012023|150,00|2,47|11,40|12345678901234|JOSE DA SILVA|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SomenteObrigatorios_PreservaTextoCanonico()
    {
        // VL_PIS, VL_COFINS, CPF_CNPJ e NOM_ADQ são OC — dispensados para quem entrega EFD-Contribuições.
        const string sped =
            "|C460|2D|00|56789|15032023|89,90|||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_DocumentoCancelado_PreservaTextoCanonico()
    {
        // Para cupom cancelado (COD_SIT = 02), somente COD_MOD, COD_SIT e NUM_DOC relevantes;
        // os demais campos ficam zerados/vazios conforme orientação do guia.
        const string sped =
            "|C460|60|02|99|20062023|0,00|||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

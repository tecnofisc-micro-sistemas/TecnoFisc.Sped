using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.042 — exercita a forma do <see cref="RegistroC112"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 68): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC112Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC112).Assembly);

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
    public void Atributo_DeclaraC112_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC112).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C112");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC112Com7CamposNaOrdem()
    {
        _catalogo.TentarObter("C112".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C112");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodDa", "Uf", "NumDa", "CodAut", "VlDa", "DtVcto", "DtPgto",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 7));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C112".AsSpan(), out var meta);
        var registro = (RegistroC112)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());                    // CodDa
        meta.Campos[1].Definidor(registro, "SP".AsSpan());                   // Uf
        meta.Campos[2].Definidor(registro, "123456".AsSpan());               // NumDa
        meta.Campos[3].Definidor(registro, "98765432101234567890".AsSpan()); // CodAut
        meta.Campos[4].Definidor(registro, "150.50".AsSpan());               // VlDa
        meta.Campos[5].Definidor(registro, "31012024".AsSpan());             // DtVcto
        meta.Campos[6].Definidor(registro, "05022024".AsSpan());             // DtPgto

        registro.CodDa.Should().Be(CodigoModeloDocumentoArrecadacao.Gnre);
        registro.Uf.Should().Be("SP");
        registro.NumDa.Should().Be("123456");
        registro.CodAut.Should().Be("98765432101234567890");
        registro.VlDa.Should().Be(150.50m);
        registro.DtVcto.Should().Be(new DateOnly(2024, 1, 31));
        registro.DtPgto.Should().Be(new DateOnly(2024, 2, 5));
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C112".AsSpan(), out var meta);
        var registro = (RegistroC112)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, Span<char>.Empty);  // NumDa (string nullable)
        meta.Campos[3].Definidor(registro, Span<char>.Empty);  // CodAut (string nullable)

        registro.NumDa.Should().BeNull();
        registro.CodAut.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // GNRE para SP, com número e autenticação bancária preenchidos.
        const string sped = "|C112|1|SP|123456|98765432101234567890|150,50|31012024|05022024|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // Documento estadual sem número e sem autenticação bancária.
        const string sped = "|C112|0|RJ|||1000,00|15032024|15032024|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("0", CodigoModeloDocumentoArrecadacao.DocumentoEstadual)]
    [InlineData("1", CodigoModeloDocumentoArrecadacao.Gnre)]
    public void Definidor_CodDaCobertosEnum(string valor, CodigoModeloDocumentoArrecadacao esperado)
    {
        _catalogo.TentarObter("C112".AsSpan(), out var meta);
        var registro = (RegistroC112)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, valor.AsSpan());

        registro.CodDa.Should().Be(esperado);
    }
}

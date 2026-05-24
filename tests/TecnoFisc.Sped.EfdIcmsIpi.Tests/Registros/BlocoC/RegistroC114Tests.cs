using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.044 — exercita a forma do <see cref="RegistroC114"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 70): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC114Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC114).Assembly);

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
    public void Atributo_DeclaraC114_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC114).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C114");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC114Com5CamposNaOrdem()
    {
        _catalogo.TentarObter("C114".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C114");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodMod", "EcfFab", "EcfCx", "NumDoc", "DtDoc",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 5));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C114".AsSpan(), out var meta);
        var registro = (RegistroC114)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "02".AsSpan());                    // CodMod
        meta.Campos[1].Definidor(registro, "SN1234567890123456789".AsSpan()); // EcfFab — 21 chars
        meta.Campos[2].Definidor(registro, "001".AsSpan());                   // EcfCx
        meta.Campos[3].Definidor(registro, "000000042".AsSpan());             // NumDoc
        meta.Campos[4].Definidor(registro, "01012024".AsSpan());              // DtDoc

        registro.CodMod.Should().Be("02");
        registro.EcfFab.Should().Be("SN1234567890123456789");
        registro.EcfCx.Should().Be(1);
        registro.NumDoc.Should().Be(42);
        registro.DtDoc.Should().Be(new DateOnly(2024, 1, 1));
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C114".AsSpan(), out var meta);
        var registro = (RegistroC114)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);  // CodMod
        meta.Campos[1].Definidor(registro, Span<char>.Empty);  // EcfFab
        meta.Campos[2].Definidor(registro, Span<char>.Empty);  // EcfCx
        meta.Campos[3].Definidor(registro, Span<char>.Empty);  // NumDoc

        registro.CodMod.Should().BeNull();
        registro.EcfFab.Should().BeNull();
        registro.EcfCx.Should().BeNull();
        registro.NumDoc.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Cupom ECF modelo 02, série de fabricação de 21 chars, caixa 1, COO 42.
        const string sped = "|C114|02|SN1234567890123456789|1|42|01012024|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComModeloCupomFiscalEletronico_PreservaTextoCanonico()
    {
        // ECF modelo 2E (cupom de bilhete de passagem) com numeração diferente.
        const string sped = "|C114|2E|FABR00000000000000001|5|1234|15032024|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

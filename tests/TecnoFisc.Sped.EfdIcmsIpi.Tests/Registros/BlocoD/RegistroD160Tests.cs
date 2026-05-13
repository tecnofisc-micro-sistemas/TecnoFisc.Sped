using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.122 — exercita a forma do <see cref="RegistroD160"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.2.2 (p. 180): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD160Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD160).Assembly);

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
    public void Atributo_DeclaraD160_Nivel3_BlocoD()
    {
        var atributo = typeof(RegistroD160).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D160");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD160Com7CamposNaOrdem()
    {
        _catalogo.TentarObter("D160".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D160");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "Despacho", "CnpjCpfRem", "IeRem", "CodMunOri",
            "CnpjCpfDest", "IeDest", "CodMunDest",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 7));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D160".AsSpan(), out var meta);
        var registro = (RegistroD160)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "12345".AsSpan());            // Despacho
        meta.Campos[1].Definidor(registro, "12345678901234".AsSpan());   // CnpjCpfRem
        meta.Campos[2].Definidor(registro, "SP123456789".AsSpan());      // IeRem
        meta.Campos[3].Definidor(registro, "3550308".AsSpan());          // CodMunOri
        meta.Campos[4].Definidor(registro, "98765432100001".AsSpan());   // CnpjCpfDest
        meta.Campos[5].Definidor(registro, "MG987654321".AsSpan());      // IeDest
        meta.Campos[6].Definidor(registro, "3304557".AsSpan());          // CodMunDest

        registro.Despacho.Should().Be("12345");
        registro.CnpjCpfRem.Should().Be("12345678901234");
        registro.IeRem.Should().Be("SP123456789");
        registro.CodMunOri.Should().Be(3550308);
        registro.CnpjCpfDest.Should().Be("98765432100001");
        registro.IeDest.Should().Be("MG987654321");
        registro.CodMunDest.Should().Be(3304557);
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("D160".AsSpan(), out var meta);
        var registro = (RegistroD160)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);  // Despacho
        meta.Campos[1].Definidor(registro, Span<char>.Empty);  // CnpjCpfRem
        meta.Campos[2].Definidor(registro, Span<char>.Empty);  // IeRem
        meta.Campos[4].Definidor(registro, Span<char>.Empty);  // CnpjCpfDest
        meta.Campos[5].Definidor(registro, Span<char>.Empty);  // IeDest

        registro.Despacho.Should().BeNull();
        registro.CnpjCpfRem.Should().BeNull();
        registro.IeRem.Should().BeNull();
        registro.CnpjCpfDest.Should().BeNull();
        registro.IeDest.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Carga com remetente e destinatário identificados, origem SP, destino RJ.
        const string sped = "|D160|12345|12345678901234|SP123456789|3550308|98765432100001|MG987654321|3304557|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // Apenas municípios de origem e destino informados.
        const string sped = "|D160||||3550308|||3304557|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_TransporteExterior_PreservaTextoCanonico()
    {
        // Origem e destino no Exterior — código 9999999.
        const string sped = "|D160||||9999999|||9999999|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCpf_PreservaTextoCanonico()
    {
        // Remetente e destinatário identificados por CPF (11 dígitos).
        const string sped = "|D160||12345678901||3550308|98765432100||3304557|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

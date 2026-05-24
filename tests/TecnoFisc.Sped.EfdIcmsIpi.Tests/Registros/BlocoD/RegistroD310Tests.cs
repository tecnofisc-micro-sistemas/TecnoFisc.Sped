using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.132 — exercita a forma do <see cref="RegistroD310"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 185): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD310Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD310).Assembly);

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
    public void Atributo_DeclaraD310_Nivel3_BlocoD()
    {
        var atributo = typeof(RegistroD310).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D310");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD310Com4CamposNaOrdem()
    {
        _catalogo.TentarObter("D310".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D310");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodMunOrig", "VlServ", "VlBcIcms", "VlIcms"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D310".AsSpan(), out var meta);
        var registro = (RegistroD310)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "3550308".AsSpan()); // CodMunOrig
        meta.Campos[1].Definidor(registro, "1500,00".AsSpan()); // VlServ
        meta.Campos[2].Definidor(registro, "1350,00".AsSpan()); // VlBcIcms
        meta.Campos[3].Definidor(registro, "162,00".AsSpan());  // VlIcms

        registro.CodMunOrig.Should().Be(3550308);
        registro.VlServ.Should().Be(1500.00m);
        registro.VlBcIcms.Should().Be(1350.00m);
        registro.VlIcms.Should().Be(162.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D310".AsSpan(), out var meta);
        var registro = (RegistroD310)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty); // CodMunOrig
        meta.Campos[2].Definidor(registro, Span<char>.Empty); // VlBcIcms
        meta.Campos[3].Definidor(registro, Span<char>.Empty); // VlIcms

        registro.CodMunOrig.Should().BeNull();
        registro.VlBcIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Município de São Paulo (3550308), com base e ICMS preenchidos.
        const string sped = "|D310|3550308|1500,00|1350,00|162,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemBaseEIcms_PreservaTextoCanonico()
    {
        // Operação não tributada: VL_BC_ICMS e VL_ICMS vazios (OC).
        const string sped = "|D310|3304557|2000,00|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

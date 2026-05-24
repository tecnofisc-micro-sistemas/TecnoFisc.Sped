using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.117 — exercita a forma do <see cref="RegistroD110"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 169): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD110Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD110).Assembly);

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
    public void Atributo_DeclaraD110_Nivel3_BlocoD()
    {
        var atributo = typeof(RegistroD110).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D110");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD110Com4CamposNaOrdem()
    {
        _catalogo.TentarObter("D110".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D110");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "NumItem", "CodItem", "VlServ", "VlOut",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 4));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D110".AsSpan(), out var meta);
        var registro = (RegistroD110)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());          // NumItem
        meta.Campos[1].Definidor(registro, "SERV-FRETE".AsSpan()); // CodItem
        meta.Campos[2].Definidor(registro, "1500.00".AsSpan());    // VlServ
        meta.Campos[3].Definidor(registro, "50.00".AsSpan());      // VlOut

        registro.NumItem.Should().Be(1);
        registro.CodItem.Should().Be("SERV-FRETE");
        registro.VlServ.Should().Be(1500.00m);
        registro.VlOut.Should().Be(50.00m);

    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D110".AsSpan(), out var meta);
        var registro = (RegistroD110)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, Span<char>.Empty); // VlOut opcional
        registro.VlOut.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Item 1 da NF de serviço de transporte (cód. 07): serviço principal + outros valores.
        const string sped = "|D110|1|SERV-FRETE|1500,00|50,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemVlOut_PreservaTextoCanonico()
    {
        // Item sem outros valores (VlOut omitido).
        const string sped = "|D110|2|CARGA-001|800,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

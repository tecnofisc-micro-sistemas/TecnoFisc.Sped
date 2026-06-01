using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.062 — exercita a forma do <see cref="RegistroC179"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 90): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC179Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC179).Assembly);

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
    public void Atributo_DeclaraC179_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC179).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C179");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC179Com5CamposNaOrdem()
    {
        _catalogo.TentarObter("C179".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C179");
        meta.Campos.Select(c => c.Nome).Should().Equal(["BcStOrigDest", "IcmsStRep", "IcmsStCompl", "BcRet", "IcmsRet"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C179".AsSpan(), out var meta);
        var registro = (RegistroC179)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1000,00".AsSpan());  // BcStOrigDest
        meta.Campos[1].Definidor(registro, "120,00".AsSpan());   // IcmsStRep
        meta.Campos[2].Definidor(registro, "30,00".AsSpan());    // IcmsStCompl
        meta.Campos[3].Definidor(registro, "500,00".AsSpan());   // BcRet
        meta.Campos[4].Definidor(registro, "-15,00".AsSpan());   // IcmsRet (N' — pode ser negativo)

        registro.BcStOrigDest.Should().Be(1000.00m);
        registro.IcmsStRep.Should().Be(120.00m);
        registro.IcmsStCompl.Should().Be(30.00m);
        registro.BcRet.Should().Be(500.00m);
        registro.IcmsRet.Should().Be(-15.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C179".AsSpan(), out var meta);
        var registro = (RegistroC179)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.BcStOrigDest.Should().BeNull();
        registro.IcmsStRep.Should().BeNull();
        registro.IcmsStCompl.Should().BeNull();
        registro.BcRet.Should().BeNull();
        registro.IcmsRet.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C179|1000,00|120,00|30,00|500,00|-15,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // Operação sem complemento e sem retenção intermediária; IcmsRet nulo no trailing = omitido.
        const string sped = "|C179|2500,00|300,00||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

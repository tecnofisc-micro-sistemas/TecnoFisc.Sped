using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.137 — exercita a forma do <see cref="RegistroD370"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 186): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD370Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD370).Assembly);

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
    public void Atributo_DeclaraD370_Nivel5_BlocoD()
    {
        var atributo = typeof(RegistroD370).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D370");
        atributo.Nivel.Should().Be(5);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD370Com5CamposNaOrdem()
    {
        _catalogo.TentarObter("D370".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D370");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodMunOrig", "VlServ", "QtdBilh", "VlBcIcms", "VlIcms"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D370".AsSpan(), out var meta);
        var registro = (RegistroD370)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "3550308".AsSpan()); // CodMunOrig
        meta.Campos[1].Definidor(registro, "1000,00".AsSpan()); // VlServ
        meta.Campos[2].Definidor(registro, "250".AsSpan());     // QtdBilh
        meta.Campos[3].Definidor(registro, "900,00".AsSpan());  // VlBcIcms
        meta.Campos[4].Definidor(registro, "81,00".AsSpan());   // VlIcms

        registro.CodMunOrig.Should().Be("3550308");
        registro.VlServ.Should().Be(1000.00m);
        registro.QtdBilh.Should().Be(250);
        registro.VlBcIcms.Should().Be(900.00m);
        registro.VlIcms.Should().Be(81.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D370".AsSpan(), out var meta);
        var registro = (RegistroD370)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty); // CodMunOrig
        meta.Campos[1].Definidor(registro, Span<char>.Empty); // VlServ
        meta.Campos[2].Definidor(registro, Span<char>.Empty); // QtdBilh
        meta.Campos[3].Definidor(registro, Span<char>.Empty); // VlBcIcms
        meta.Campos[4].Definidor(registro, Span<char>.Empty); // VlIcms

        registro.CodMunOrig.Should().BeNull();
        registro.VlServ.Should().BeNull();
        registro.QtdBilh.Should().BeNull();
        registro.VlBcIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Complemento do documento informado com todos os campos preenchidos.
        const string sped = "|D370|3550308|1000,00|250|900,00|81,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposCondicionaisVazios_PreservaTextoCanonico()
    {
        // Complemento sem base de cálculo e valor do ICMS (campos OC — condicionais).
        const string sped = "|D370|3550308|500,00|100|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.138 — exercita a forma do <see cref="RegistroD390"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.2.2 (p. 194): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD390Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD390).Assembly);

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
    public void Atributo_DeclaraD390_Nivel4_BlocoD()
    {
        var atributo = typeof(RegistroD390).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D390");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD390Com10CamposNaOrdem()
    {
        _catalogo.TentarObter("D390".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D390");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["CstIcms", "Cfop", "AliqIcms", "VlOpr", "VlBcIssqn", "AliqIssqn", "VlIssqn", "VlBcIcms", "VlIcms", "CodObs"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D390".AsSpan(), out var meta);
        var registro = (RegistroD390)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "040".AsSpan());      // CstIcms
        meta.Campos[1].Definidor(registro, "5102".AsSpan());     // Cfop
        meta.Campos[2].Definidor(registro, "12,00".AsSpan());    // AliqIcms
        meta.Campos[3].Definidor(registro, "5000,00".AsSpan());  // VlOpr
        meta.Campos[4].Definidor(registro, "200,00".AsSpan());   // VlBcIssqn
        meta.Campos[5].Definidor(registro, "5,00".AsSpan());     // AliqIssqn
        meta.Campos[6].Definidor(registro, "10,00".AsSpan());    // VlIssqn
        meta.Campos[7].Definidor(registro, "4500,00".AsSpan());  // VlBcIcms
        meta.Campos[8].Definidor(registro, "540,00".AsSpan());   // VlIcms
        meta.Campos[9].Definidor(registro, "OBS001".AsSpan());   // CodObs

        registro.CstIcms.Should().Be(40);
        registro.Cfop.Should().Be(Cfop.Create("5102".AsSpan()));
        registro.AliqIcms.Should().Be(12.00m);
        registro.VlOpr.Should().Be(5000.00m);
        registro.VlBcIssqn.Should().Be(200.00m);
        registro.AliqIssqn.Should().Be(5.00m);
        registro.VlIssqn.Should().Be(10.00m);
        registro.VlBcIcms.Should().Be(4500.00m);
        registro.VlIcms.Should().Be(540.00m);
        registro.CodObs.Should().Be("OBS001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D390".AsSpan(), out var meta);
        var registro = (RegistroD390)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty); // CstIcms
        meta.Campos[1].Definidor(registro, Span<char>.Empty); // Cfop
        meta.Campos[2].Definidor(registro, Span<char>.Empty); // AliqIcms
        meta.Campos[3].Definidor(registro, Span<char>.Empty); // VlOpr
        meta.Campos[4].Definidor(registro, Span<char>.Empty); // VlBcIssqn
        meta.Campos[5].Definidor(registro, Span<char>.Empty); // AliqIssqn
        meta.Campos[6].Definidor(registro, Span<char>.Empty); // VlIssqn
        meta.Campos[7].Definidor(registro, Span<char>.Empty); // VlBcIcms
        meta.Campos[8].Definidor(registro, Span<char>.Empty); // VlIcms
        meta.Campos[9].Definidor(registro, Span<char>.Empty); // CodObs

        registro.CstIcms.Should().BeNull();
        registro.Cfop.Should().BeNull();
        registro.AliqIcms.Should().BeNull();
        registro.VlOpr.Should().BeNull();
        registro.VlBcIssqn.Should().BeNull();
        registro.AliqIssqn.Should().BeNull();
        registro.VlIssqn.Should().BeNull();
        registro.VlBcIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
        registro.CodObs.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // CstIcms int? serializa sem zero-padding — forma canônica é "40" não "040".
        const string sped = "|D390|40|5102|12,00|5000,00|200,00|5,00|10,00|4500,00|540,00|OBS001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposCondicionaisVazios_PreservaTextoCanonico()
    {
        // Registro analítico sem ISSQN e sem observação (campos OC ausentes). CST "40" sem zero-padding.
        const string sped = "|D390|40|5102||5000,00||||4500,00|540,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemAliquotaIcms_PreservaTextoCanonico()
    {
        // CST 041 (isento) — alíquota ICMS ausente, sem ISSQN. CstIcms int? serializa como "41".
        const string sped = "|D390|41|5102||3000,00||||3000,00|0,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

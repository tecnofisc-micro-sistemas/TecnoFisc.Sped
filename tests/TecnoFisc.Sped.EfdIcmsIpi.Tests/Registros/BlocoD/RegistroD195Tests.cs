using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.128 — exercita a forma do <see cref="RegistroD195"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 179): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD195Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD195).Assembly);

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
    public void Atributo_DeclaraD195_Nivel3_BlocoD()
    {
        var atributo = typeof(RegistroD195).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D195");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD195Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("D195".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D195");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodObs", "TxtCompl"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D195".AsSpan(), out var meta);
        var registro = (RegistroD195)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "OBS001".AsSpan());                    // CodObs
        meta.Campos[1].Definidor(registro, "Ajuste ICMS diferencial".AsSpan());   // TxtCompl

        registro.CodObs.Should().Be("OBS001");
        registro.TxtCompl.Should().Be("Ajuste ICMS diferencial");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D195".AsSpan(), out var meta);
        var registro = (RegistroD195)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodObs.Should().BeNull();
        registro.TxtCompl.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|D195|OBS001|Ajuste diferencial de aliquota|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemTxtCompl_PreservaTextoCanonico()
    {
        // TXT_COMPL é OC — registro válido sem complemento textual.
        const string sped =
            "|D195|OBS001||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

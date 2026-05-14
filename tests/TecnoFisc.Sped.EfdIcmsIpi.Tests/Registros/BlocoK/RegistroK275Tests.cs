using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 8.206 — exercita a forma do <see cref="RegistroK275"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 261-262): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroK275Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroK275).Assembly);

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
    public void Atributo_DeclaraK275_Nivel4_BlocoK()
    {
        var atributo = typeof(RegistroK275).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K275");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK275ComQuatroCamposNaOrdem()
    {
        _catalogo.TentarObter("K275".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K275");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodItem",
            "QtdCorPos",
            "QtdCorNeg",
            "CodInsSubst",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K275".AsSpan(), out var meta);
        var registro = (RegistroK275)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "INSUMO-CORRIGIDO".AsSpan());
        meta.Campos[1].Definidor(registro, "1,234567".AsSpan());
        meta.Campos[2].Definidor(registro, "0,765432".AsSpan());
        meta.Campos[3].Definidor(registro, "INSUMO-SUBSTITUIDO".AsSpan());

        registro.CodItem.Should().Be("INSUMO-CORRIGIDO");
        registro.QtdCorPos.Should().Be(1.234567m);
        registro.QtdCorNeg.Should().Be(0.765432m);
        registro.CodInsSubst.Should().Be("INSUMO-SUBSTITUIDO");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("K275".AsSpan(), out var meta);
        var registro = (RegistroK275)meta!.Fabrica();

        meta!.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.QtdCorPos.Should().BeNull();
        registro.QtdCorNeg.Should().BeNull();
        registro.CodInsSubst.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|K275|INSUMO-CORRIGIDO|1,234567|0,765432|INSUMO-SUBSTITUIDO|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemQuantidadeNegativaESemSubstituicao_PreservaTextoCanonico()
    {
        const string sped = "|K275|INSUMO-CORRIGIDO|2,000000|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

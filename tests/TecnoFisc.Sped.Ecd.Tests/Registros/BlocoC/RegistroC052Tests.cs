using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoC;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 10.013 — exercita a forma do <see cref="RegistroC052"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 95): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroC052Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000).Assembly);

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
    public void Atributo_DeclaraC052_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC052).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C052");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC052Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("C052".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C052");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodCcus", "CodAgl",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C052".AsSpan(), out var meta);
        var registro = (RegistroC052)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "CC001".AsSpan());
        meta.Campos[1].Definidor(registro, "AGL001".AsSpan());

        registro.CodCcus.Should().Be("CC001");
        registro.CodAgl.Should().Be("AGL001");
    }

    [Fact]
    public void Definidor_CampoOpcionalVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C052".AsSpan(), out var meta);
        var registro = (RegistroC052)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCcus

        registro.CodCcus.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C052|CC001|AGL001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCodCcusVazio_PreservaTextoCanonico()
    {
        const string sped =
            "|C052||AGL001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

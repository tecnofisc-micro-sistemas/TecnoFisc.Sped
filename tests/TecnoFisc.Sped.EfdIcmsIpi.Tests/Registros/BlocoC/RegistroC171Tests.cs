using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.054 — exercita a forma do <see cref="RegistroC171"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 82): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC171Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC171).Assembly);

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
    public void Atributo_DeclaraC171_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC171).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C171");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC171Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("C171".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C171");
        meta.Campos.Select(c => c.Nome).Should().Equal(["NumTanque", "Qtde"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C171".AsSpan(), out var meta);
        var registro = (RegistroC171)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "001".AsSpan());     // NumTanque
        meta.Campos[1].Definidor(registro, "1500,000".AsSpan()); // Qtde

        registro.NumTanque.Should().Be("001");
        registro.Qtde.Should().Be(1500.000m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C171".AsSpan(), out var meta);
        var registro = (RegistroC171)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.NumTanque.Should().BeNull();
        registro.Qtde.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Armazenamento de combustível com tanque e volume preenchidos.
        const string sped = "|C171|001|1500,000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposVazios_PreservaTextoCanonico()
    {
        // Ambos os campos opcionais ausentes.
        const string sped = "|C171|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

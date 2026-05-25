using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoI;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoI;

/// <summary>
/// Sub-stage 10.030 — exercita a forma do <see cref="RegistroI100"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 130–131): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroI100Tests
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
    public void Atributo_DeclaraI100_Nivel3_BlocoI()
    {
        var atributo = typeof(RegistroI100).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I100");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI100Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("I100".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I100");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "DtAlt", "CodCcus", "Ccus",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I100".AsSpan(), out var meta);
        var registro = (RegistroI100)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01012005".AsSpan());
        meta.Campos[1].Definidor(registro, "CC2328-001".AsSpan());
        meta.Campos[2].Definidor(registro, "DIVISÃO A".AsSpan());

        registro.DtAlt.Should().Be(new DateOnly(2005, 1, 1));
        registro.CodCcus.Should().Be("CC2328-001");
        registro.Ccus.Should().Be("DIVISÃO A");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("I100".AsSpan(), out var meta);
        var registro = (RegistroI100)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodCcus.Should().BeNull();
        registro.Ccus.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 131): DT_ALT=01012005, COD_CCUS=CC2328-001, CCUS=DIVISÃO A
        const string sped = "|I100|01012005|CC2328-001|DIVISÃO A|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComOutroCentro_PreservaTextoCanonico()
    {
        const string sped = "|I100|31122024|ADM001|ADMINISTRAÇÃO GERAL|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

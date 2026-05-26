using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoI;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoI;

/// <summary>
/// Sub-stage 10.026 — exercita a forma do <see cref="RegistroI051"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 123–124): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroI051Tests
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
    public void Atributo_DeclaraI051_Nivel4_BlocoI()
    {
        var atributo = typeof(RegistroI051).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I051");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI051Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("I051".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I051");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodCcus", "CodCtaRef",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I051".AsSpan(), out var meta);
        var registro = (RegistroI051)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "CC001".AsSpan());
        meta.Campos[1].Definidor(registro, "1.02.03.01.08".AsSpan());

        registro.CodCcus.Should().Be("CC001");
        registro.CodCtaRef.Should().Be("1.02.03.01.08");
    }

    [Fact]
    public void Definidor_CampoOpcionalVazio_DevolveNulo()
    {
        _catalogo.TentarObter("I051".AsSpan(), out var meta);
        var registro = (RegistroI051)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCcus

        registro.CodCcus.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Com centro de custo e código referencial
        const string sped = "|I051|CC001|1.02.03.01.08|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ExemploDoManual_SemCodCcus_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 124): COD_CCUS vazio, COD_CTA_REF = 11100009
        const string sped = "|I051||11100009|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

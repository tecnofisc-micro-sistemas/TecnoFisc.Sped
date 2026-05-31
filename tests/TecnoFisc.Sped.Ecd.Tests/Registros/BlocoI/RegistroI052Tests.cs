using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoI;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoI;

/// <summary>
/// Sub-stage 10.027 — exercita a forma do <see cref="RegistroI052"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 125–126): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroI052Tests
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
    public void Atributo_DeclaraI052_Nivel4_BlocoI()
    {
        var atributo = typeof(RegistroI052).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I052");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI052Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("I052".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I052");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodCcus", "CodAgl",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I052".AsSpan(), out var meta);
        var registro = (RegistroI052)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "CC001".AsSpan());
        meta.Campos[1].Definidor(registro, "1.1".AsSpan());

        registro.CodCcus.Should().Be("CC001");
        registro.CodAgl.Should().Be("1.1");
    }

    [Fact]
    public void Definidor_CampoOpcionalVazio_DevolveNulo()
    {
        _catalogo.TentarObter("I052".AsSpan(), out var meta);
        var registro = (RegistroI052)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCcus

        registro.CodCcus.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Com centro de custo e código de aglutinação
        const string sped = "|I052|CC001|1.1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ExemploDoManual_SemCodCcus_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 126): COD_CCUS vazio, COD_AGL = 1.1
        const string sped = "|I052||1.1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

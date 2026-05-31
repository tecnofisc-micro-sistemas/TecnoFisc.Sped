using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoC;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 10.018 — exercita a forma do <see cref="RegistroC990"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 101): metadados de catálogo, mapeamento de campo e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroC990Tests
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
    public void Atributo_DeclaraC990_Nivel1_BlocoC()
    {
        var atributo = typeof(RegistroC990).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C990");
        atributo.Nivel.Should().Be(1);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC990ComUmCampoNaOrdem()
    {
        _catalogo.TentarObter("C990".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C990");
        meta.Campos.Select(c => c.Nome).Should().Equal(["QtdLin0"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2]);
    }

    [Fact]
    public void Definidor_AtribuiQtdLin0()
    {
        _catalogo.TentarObter("C990".AsSpan(), out var meta);
        var registro = (RegistroC990)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "200".AsSpan());

        registro.QtdLin0.Should().Be(200);
    }

    [Fact]
    public async Task RoundTrip_ComDuzentasLinhas_PreservaTextoCanonico()
    {
        const string sped = "|C990|200|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComUmaLinha_PreservaTextoCanonico()
    {
        const string sped = "|C990|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

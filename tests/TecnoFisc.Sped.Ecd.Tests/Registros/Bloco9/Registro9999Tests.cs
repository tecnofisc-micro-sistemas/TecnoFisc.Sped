using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.Bloco9;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.Bloco9;

/// <summary>
/// Sub-stage 10.072 — exercita a forma do <see cref="Registro9999"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 233): metadados de catálogo, mapeamento de campo e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class Registro9999Tests
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
    public void Atributo_Declara9999_Nivel1_Bloco9()
    {
        var atributo = typeof(Registro9999).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("9999");
        atributo.Nivel.Should().Be(1);
        atributo.Bloco.Should().Be("9");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro9999ComUmCampoNaOrdem()
    {
        _catalogo.TentarObter("9999".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("9999");
        meta.Campos.Select(c => c.Nome).Should().Equal(["QtdLin"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2]);
        meta.Campos[0].Obrigatorio.Should().BeTrue();
    }

    [Fact]
    public void Definidor_AtribuiQtdLin()
    {
        _catalogo.TentarObter("9999".AsSpan(), out var meta);
        var registro = (Registro9999)meta!.Fabrica();

        // Exemplo do manual (p. 233): |9999|10000|
        meta.Campos[0].Definidor(registro, "10000".AsSpan());

        registro.QtdLin.Should().Be(10000);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 233): |9999|10000|
        const string sped = "|9999|10000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComUmaLinha_PreservaTextoCanonico()
    {
        const string sped = "|9999|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

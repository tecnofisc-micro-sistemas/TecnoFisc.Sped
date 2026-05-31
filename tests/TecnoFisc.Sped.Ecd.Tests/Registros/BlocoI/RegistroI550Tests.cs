using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoI;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoI;

/// <summary>
/// Sub-stage 10.042 — exercita a forma do <see cref="RegistroI550"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 164–166): metadados de catálogo, mapeamento do campo
/// variádico RZ_CONT e invariante de round-trip parse → gerar → texto idêntico. O campo
/// variádico captura tudo que resta na linha após o REG como string pipe-joined. Pacote
/// read-only — o round-trip usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroI550Tests
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
    public void Atributo_DeclaraI550_Nivel3_BlocoI()
    {
        var atributo = typeof(RegistroI550).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I550");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI550Com1CampoNaOrdem()
    {
        _catalogo.TentarObter("I550".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I550");
        meta.Campos.Select(c => c.Nome).Should().Equal(["RzCont"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2]);
        meta.Campos[0].CapturaTudo.Should().BeTrue();
    }

    [Fact]
    public void Definidor_AtribuiRzCont()
    {
        _catalogo.TentarObter("I550".AsSpan(), out var meta);
        var registro = (RegistroI550)meta!.Fabrica();

        // Conteúdo pipe-joined de 5 campos definidos no I510 (exemplo do manual)
        meta.Campos[0].Definidor(registro, "101|INSUMO1|10,10|100|1010,00".AsSpan());

        registro.RzCont.Should().Be("101|INSUMO1|10,10|100|1010,00");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("I550".AsSpan(), out var meta);
        var registro = (RegistroI550)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.RzCont.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComConteudoVariadico_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 166): 5 campos definidos no I510, valores separados por |
        const string sped = "|I550|101|INSUMO1|10,10|100|1010,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemConteudoVariadico_PreservaTextoCanonico()
    {
        // Linha I550 sem campos variádicos (conteúdo vazio)
        const string sped = "|I550||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposVaziosIntermediarios_PreservaTextoCanonico()
    {
        // Campos numéricos sem valor (vazios entre pipes)
        const string sped = "|I550|2001|PRODUTO2||100|2020|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

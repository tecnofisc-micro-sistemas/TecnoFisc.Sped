using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoI;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoI;

/// <summary>
/// Sub-stage 10.043 — exercita a forma do <see cref="RegistroI555"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 167–168): metadados de catálogo, mapeamento do campo
/// variádico RZ_CONT_TOT e invariante de round-trip parse → gerar → texto idêntico. Pacote
/// read-only — o round-trip usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroI555Tests
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
    public void Atributo_DeclaraI555_Nivel4_BlocoI()
    {
        var atributo = typeof(RegistroI555).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I555");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI555Com1CampoNaOrdem()
    {
        _catalogo.TentarObter("I555".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I555");
        meta.Campos.Select(c => c.Nome).Should().Equal(["RzContTot"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2]);
        meta.Campos[0].CapturaTudo.Should().BeTrue();
    }

    [Fact]
    public void Definidor_AtribuiRzContTot()
    {
        _catalogo.TentarObter("I555".AsSpan(), out var meta);
        var registro = (RegistroI555)meta!.Fabrica();

        // Exemplo do manual (p. 168): 5 campos do I510, apenas chave e totalizados preenchidos
        meta.Campos[0].Definidor(registro, "TOTAL|PRODUTO ACABADO|30,30||3030".AsSpan());

        registro.RzContTot.Should().Be("TOTAL|PRODUTO ACABADO|30,30||3030");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("I555".AsSpan(), out var meta);
        var registro = (RegistroI555)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.RzContTot.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTotaisVariadicos_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 168): campos chave + totalizados, VR_UNIT vazio
        const string sped = "|I555|TOTAL|PRODUTO ACABADO|30,30||3030|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemConteudoVariadico_PreservaTextoCanonico()
    {
        const string sped = "|I555||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposVaziosIntermediarios_PreservaTextoCanonico()
    {
        // Campos não totalizados ficam vazios entre pipes
        const string sped = "|I555|TOTAL|||30,30||3030|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

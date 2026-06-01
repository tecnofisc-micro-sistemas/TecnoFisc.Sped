using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.Bloco9;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.Bloco9;

/// <summary>
/// Sub-stage 10.070 — exercita a forma do <see cref="Registro9900"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 231): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class Registro9900Tests
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
    public void Atributo_Declara9900_Nivel2_Bloco9()
    {
        var atributo = typeof(Registro9900).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("9900");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("9");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro9900Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("9900".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("9900");
        meta.Campos.Select(c => c.Nome).Should().Equal(["RegBlc", "QtdRegBlc"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
        meta.Campos[0].Tamanho.Should().Be(4);
        meta.Campos[0].Obrigatorio.Should().BeTrue();
        meta.Campos[1].Obrigatorio.Should().BeTrue();
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("9900".AsSpan(), out var meta);
        var registro = (Registro9900)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0150".AsSpan());
        meta.Campos[1].Definidor(registro, "10".AsSpan());

        registro.RegBlc.Should().Be("0150");
        registro.QtdRegBlc.Should().Be(10);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("9900".AsSpan(), out var meta);
        var registro = (Registro9900)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.RegBlc.Should().BeNull();
        registro.QtdRegBlc.Should().Be(0);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 231): |9900|0150|10|
        const string sped = "|9900|0150|10|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_RegistroPosterior9999_PreservaTextoCanonico()
    {
        const string sped = "|9900|9999|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_RegistroI200ComMuitasOcorrencias_PreservaTextoCanonico()
    {
        const string sped = "|9900|I200|500|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

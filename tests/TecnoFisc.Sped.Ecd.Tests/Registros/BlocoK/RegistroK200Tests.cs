using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoK;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 10.063 — exercita a forma do <see cref="RegistroK200"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 220–222): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroK200Tests
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
    public void Atributo_DeclaraK200_Nivel2_BlocoK()
    {
        var atributo = typeof(RegistroK200).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K200");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK200Com6CamposNaOrdem()
    {
        _catalogo.TentarObter("K200".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K200");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["CodNat", "IndCta", "Nivel", "CodCta", "CodCtaSup", "Cta"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K200".AsSpan(), out var meta);
        var registro = (RegistroK200)meta!.Fabrica();

        // Exemplo do manual (p. 222): |K200|01|S|1|1||ATIVO|
        meta.Campos[0].Definidor(registro, "01".AsSpan());
        meta.Campos[1].Definidor(registro, "S".AsSpan());
        meta.Campos[2].Definidor(registro, "1".AsSpan());
        meta.Campos[3].Definidor(registro, "1".AsSpan());
        meta.Campos[4].Definidor(registro, "".AsSpan());
        meta.Campos[5].Definidor(registro, "ATIVO".AsSpan());

        registro.CodNat.Should().Be("01");
        registro.IndCta.Should().Be(IndicadorTipoConta.Sintetica);
        registro.Nivel.Should().Be(1);
        registro.CodCta.Should().Be("1");
        registro.CodCtaSup.Should().BeNull();
        registro.Cta.Should().Be("ATIVO");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("K200".AsSpan(), out var meta);
        var registro = (RegistroK200)meta!.Fabrica();

        meta.Campos[4].Definidor(registro, "".AsSpan());

        registro.CodCtaSup.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 222): conta sintética de ativo, nível 1, sem conta superior
        const string sped = "|K200|01|S|1|1||ATIVO|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ContaAnaliticaComSuperior_PreservaTextoCanonico()
    {
        // Conta analítica de ativo, nível 2, com conta superior informada
        const string sped = "|K200|01|A|2|1.1|1|Ativo Circulante|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ContaPassivo_PreservaTextoCanonico()
    {
        // Conta sintética de passivo (COD_NAT = 02)
        const string sped = "|K200|02|S|1|2||PASSIVO|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

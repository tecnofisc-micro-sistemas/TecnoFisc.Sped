using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoI;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoI;

/// <summary>
/// Sub-stage 10.032 — exercita a forma do <see cref="RegistroI155"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 134–140): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroI155Tests
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
    public void Atributo_DeclaraI155_Nivel4_BlocoI()
    {
        var atributo = typeof(RegistroI155).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I155");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI155Com8CamposNaOrdem()
    {
        _catalogo.TentarObter("I155".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I155");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodCta", "CodCcus",
            "VlSldIni", "IndDcIni",
            "VlDeb", "VlCred", "VlSldFin", "IndDcFin",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I155".AsSpan(), out var meta);
        var registro = (RegistroI155)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "2328.2.0001".AsSpan());  // CodCta
        meta.Campos[1].Definidor(registro, "CC001".AsSpan());        // CodCcus
        meta.Campos[2].Definidor(registro, "0,00".AsSpan());         // VlSldIni
        meta.Campos[3].Definidor(registro, "D".AsSpan());            // IndDcIni
        meta.Campos[4].Definidor(registro, "7500,00".AsSpan());      // VlDeb
        meta.Campos[5].Definidor(registro, "5000,00".AsSpan());      // VlCred
        meta.Campos[6].Definidor(registro, "2500,00".AsSpan());      // VlSldFin
        meta.Campos[7].Definidor(registro, "D".AsSpan());            // IndDcFin

        registro.CodCta.Should().Be("2328.2.0001");
        registro.CodCcus.Should().Be("CC001");
        registro.VlSldIni.Should().Be(0m);
        registro.IndDcIni.Should().Be(IndicadorDebitoCredito.Devedor);
        registro.VlDeb.Should().Be(7500m);
        registro.VlCred.Should().Be(5000m);
        registro.VlSldFin.Should().Be(2500m);
        registro.IndDcFin.Should().Be(IndicadorDebitoCredito.Devedor);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("I155".AsSpan(), out var meta);
        var registro = (RegistroI155)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodCcus opcional
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);  // IndDcIni opcional
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);  // IndDcFin opcional

        registro.CodCcus.Should().BeNull();
        registro.IndDcIni.Should().BeNull();
        registro.IndDcFin.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 140): conta 2328.2.0001, sem centro de custos,
        // saldo inicial 0, VL_DEB=7500, VL_CRED=5000, saldo final 2500 (D)
        const string sped = "|I155|2328.2.0001||0,00|D|7500,00|5000,00|2500,00|D|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCentroCustos_PreservaTextoCanonico()
    {
        // Conta com centro de custos e saldo credor
        const string sped = "|I155|1.1.1.01|CC001|1000,00|C|500,00|800,00|1300,00|C|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemIndicadores_PreservaTextoCanonico()
    {
        // Registro sem IND_DC_INI e IND_DC_FIN (campos opcionais vazios)
        const string sped = "|I155|3.1.1.05||500,00||200,00|0,00|300,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposMoedaFuncional_ParseaCamposPadrao()
    {
        // Arquivo com IDENT_MF="S": I155 tem 6 campos adicionais após o campo 09.
        // O parser descarta os campos adicionais (não fazem parte do leiaute fixo).
        // O round-trip produz apenas os 9 campos padrão.
        const string entrada = "|I155|2328.2.0001||0,00|D|7500,00|5000,00|2500,00|D|0,00|D|7500,00|5000,00|2500,00|D|\r\n";
        const string esperado = "|I155|2328.2.0001||0,00|D|7500,00|5000,00|2500,00|D|\r\n";

        var leitor = new LeitorSpedTxt(_catalogo);
        using var stream = new MemoryStream(EncodingSped.Latin1.GetBytes(entrada));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.ReadStreamingAsync(stream, TestContext.Current.CancellationToken))
            registros.Add(registro);

        var i155 = registros.OfType<RegistroI155>().Single();
        i155.CodCta.Should().Be("2328.2.0001");
        i155.VlSldIni.Should().Be(0m);
        i155.VlDeb.Should().Be(7500m);
        i155.VlCred.Should().Be(5000m);
        i155.VlSldFin.Should().Be(2500m);
        i155.IndDcFin.Should().Be(IndicadorDebitoCredito.Devedor);

        var escritor = new EscritorSpedTxt(_catalogo);
        using var saida = new MemoryStream();
        await escritor.WriteAsync(saida, registros, TestContext.Current.CancellationToken);
        EncodingSped.Latin1.GetString(saida.ToArray()).Should().Be(esperado);
    }
}

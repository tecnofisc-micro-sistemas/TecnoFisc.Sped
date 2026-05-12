using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoB;

/// <summary>
/// Sub-stage 8.028 — exercita a forma do <see cref="RegistroB350"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 50-51): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroB350Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroB350).Assembly);

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerStreamingAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }

    [Fact]
    public void Atributo_DeclaraB350_Nivel2_BlocoB()
    {
        var atributo = typeof(RegistroB350).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("B350");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("B");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroB350Com10CamposNaOrdem()
    {
        _catalogo.TentarObter("B350".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("B350");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodCtd", "CtaIss", "CtaCosif", "QtdOcor", "CodServ",
            "VlCont", "VlBcIss", "AliqIss", "VlIss", "CodInfObs",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 10));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("B350".AsSpan(), out var meta);
        var registro = (RegistroB350)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "70001".AsSpan());            // CodCtd
        meta.Campos[1].Definidor(registro, "Receitas servicos".AsSpan()); // CtaIss
        meta.Campos[2].Definidor(registro, "71700009".AsSpan());          // CtaCosif
        meta.Campos[3].Definidor(registro, "5".AsSpan());                 // QtdOcor
        meta.Campos[4].Definidor(registro, "0107".AsSpan());              // CodServ
        meta.Campos[5].Definidor(registro, "10000,00".AsSpan());          // VlCont
        meta.Campos[6].Definidor(registro, "10000,00".AsSpan());          // VlBcIss
        meta.Campos[7].Definidor(registro, "5,00".AsSpan());              // AliqIss
        meta.Campos[8].Definidor(registro, "500,00".AsSpan());            // VlIss
        meta.Campos[9].Definidor(registro, "OBS001".AsSpan());            // CodInfObs

        registro.CodCtd.Should().Be("70001");
        registro.CtaIss.Should().Be("Receitas servicos");
        registro.CtaCosif.Should().Be("71700009");
        registro.QtdOcor.Should().Be(5);
        registro.CodServ.Should().Be("0107");
        registro.VlCont.Should().Be(10000.00m);
        registro.VlBcIss.Should().Be(10000.00m);
        registro.AliqIss.Should().Be(5.00m);
        registro.VlIss.Should().Be(500.00m);
        registro.CodInfObs.Should().Be("OBS001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("B350".AsSpan(), out var meta);
        var registro = (RegistroB350)meta!.Fabrica();

        meta.Campos[9].Definidor(registro, Span<char>.Empty);   // CodInfObs
        registro.CodInfObs.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|B350|70001|Receitas servicos|71700009|5|0107|10000,00|10000,00|5,00|500,00|OBS001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemObservacao_PreservaTextoCanonico()
    {
        // COD_INF_OBS ausente — campo opcional emite vazio.
        const string sped = "|B350|80002|Tarifas bancarias|71800001|3|0801|5000,00|5000,00|2,00|100,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_AliquotaMinima_PreservaTextoCanonico()
    {
        // Alíquota ISS mínima (2%) com item de serviço distinto.
        const string sped = "|B350|90003|Servicos de custodia|31900019|1|1501|20000,00|20000,00|2,00|400,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

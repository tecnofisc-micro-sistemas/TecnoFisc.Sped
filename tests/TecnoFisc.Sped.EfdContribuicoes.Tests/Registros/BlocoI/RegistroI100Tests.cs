using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoI;

public sealed class RegistroI100Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroI100).Assembly);

    [Fact]
    public void Atributo_DeclaraI100_Nivel3_BlocoI()
    {
        var atributo = typeof(RegistroI100).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I100");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI100Com11CamposNaOrdem()
    {
        _catalogo.TentarObter("I100".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I100");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "VlRec", "CstPisCofins", "VlTotDedGer", "VlTotDedEsp",
            "VlBcPis", "AliqPis", "VlPis", "VlBcCofins", "AliqCofins", "VlCofins", "InfoCompl",
        ]);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // VlRec
        meta.Campos[1].Tamanho.Should().Be(2);
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // CstPisCofins
        meta.Campos[5].Tamanho.Should().Be(8);          // AliqPis
        meta.Campos[8].Tamanho.Should().Be(8);          // AliqCofins
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I100".AsSpan(), out var meta);
        var registro = (RegistroI100)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1000000,00".AsSpan());      // VlRec
        meta.Campos[1].Definidor(registro, "01".AsSpan());             // CstPisCofins
        meta.Campos[2].Definidor(registro, "5000,00".AsSpan());         // VlTotDedGer
        meta.Campos[3].Definidor(registro, "2000,00".AsSpan());         // VlTotDedEsp
        meta.Campos[4].Definidor(registro, "993000,00".AsSpan());       // VlBcPis
        meta.Campos[5].Definidor(registro, "0,65".AsSpan());            // AliqPis
        meta.Campos[6].Definidor(registro, "6454,50".AsSpan());         // VlPis
        meta.Campos[7].Definidor(registro, "993000,00".AsSpan());       // VlBcCofins
        meta.Campos[8].Definidor(registro, "3,00".AsSpan());            // AliqCofins
        meta.Campos[9].Definidor(registro, "29790,00".AsSpan());        // VlCofins
        meta.Campos[10].Definidor(registro, "Receita financeira".AsSpan()); // InfoCompl

        registro.VlRec.Should().Be(1000000m);
        registro.CstPisCofins.Should().Be("01");
        registro.VlTotDedGer.Should().Be(5000m);
        registro.VlTotDedEsp.Should().Be(2000m);
        registro.VlBcPis.Should().Be(993000m);
        registro.AliqPis.Should().Be(0.65m);
        registro.VlPis.Should().Be(6454.50m);
        registro.VlBcCofins.Should().Be(993000m);
        registro.AliqCofins.Should().Be(3m);
        registro.VlCofins.Should().Be(29790m);
        registro.InfoCompl.Should().Be("Receita financeira");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("I100".AsSpan(), out var meta);
        var registro = (RegistroI100)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlTotDedGer
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlTotDedEsp
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlBcPis
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqPis
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlPis
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlBcCofins
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqCofins
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlCofins
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty); // InfoCompl

        registro.VlTotDedGer.Should().BeNull();
        registro.VlTotDedEsp.Should().BeNull();
        registro.VlBcPis.Should().BeNull();
        registro.AliqPis.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlBcCofins.Should().BeNull();
        registro.AliqCofins.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.InfoCompl.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|I100|1000000,00|01|5000,00|2000,00|993000,00|0,65|6454,50|993000,00|3,00|29790,00|Receita financeira|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CstIsentoSemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|I100|500000,00|07||||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComDeducoes_PreservaTextoCanonico()
    {
        const string sped =
            "|I100|2000000,00|01|80000,00|20000,00|1900000,00|0,65|12350,00|1900000,00|3,00|57000,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }
}

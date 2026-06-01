using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC175Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC175).Assembly);

    [Fact]
    public void Atributo_DeclaraC175_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC175).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C175");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC175Com17CamposNaOrdem()
    {
        _catalogo.TentarObter("C175".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C175");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "Cfop", "VlOpr", "VlDesc", "CstPis", "VlBcPis",
            "AliqPis", "QuantBcPis", "AliqPisQuant", "VlPis", "CstCofins",
            "VlBcCofins", "AliqCofins", "QuantBcCofins", "AliqCofinsQuant", "VlCofins",
            "CodCta", "InfoCompl",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18]);
        meta.Campos[0].Tamanho.Should().Be(4);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // Cfop
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // VlOpr
        meta.Campos[9].Obrigatorio.Should().BeTrue();   // CstCofins
        meta.Campos[15].Tamanho.Should().Be(255);       // CodCta
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C175".AsSpan(), out var meta);
        var registro = (RegistroC175)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "5102".AsSpan());               // Cfop
        meta.Campos[1].Definidor(registro, "1000,00".AsSpan());            // VlOpr
        meta.Campos[2].Definidor(registro, "50,00".AsSpan());              // VlDesc
        meta.Campos[3].Definidor(registro, "01".AsSpan());                 // CstPis
        meta.Campos[4].Definidor(registro, "950,00".AsSpan());             // VlBcPis
        meta.Campos[5].Definidor(registro, "0,6500".AsSpan());             // AliqPis
        meta.Campos[6].Definidor(registro, "10,000".AsSpan());             // QuantBcPis
        meta.Campos[7].Definidor(registro, "0,0100".AsSpan());             // AliqPisQuant
        meta.Campos[8].Definidor(registro, "6,18".AsSpan());               // VlPis
        meta.Campos[9].Definidor(registro, "01".AsSpan());                 // CstCofins
        meta.Campos[10].Definidor(registro, "950,00".AsSpan());            // VlBcCofins
        meta.Campos[11].Definidor(registro, "3,0000".AsSpan());            // AliqCofins
        meta.Campos[12].Definidor(registro, "10,000".AsSpan());            // QuantBcCofins
        meta.Campos[13].Definidor(registro, "0,0300".AsSpan());            // AliqCofinsQuant
        meta.Campos[14].Definidor(registro, "28,50".AsSpan());             // VlCofins
        meta.Campos[15].Definidor(registro, "3.1.01.001".AsSpan());        // CodCta
        meta.Campos[16].Definidor(registro, "Info Complementar".AsSpan()); // InfoCompl

        registro.Cfop.Should().Be(Cfop.Create("5102"));
        registro.VlOpr.Should().Be(1000m);
        registro.VlDesc.Should().Be(50m);
        registro.CstPis.Should().Be(1);
        registro.VlBcPis.Should().Be(950m);
        registro.AliqPis.Should().Be(0.65m);
        registro.QuantBcPis.Should().Be(10m);
        registro.AliqPisQuant.Should().Be(0.01m);
        registro.VlPis.Should().Be(6.18m);
        registro.CstCofins.Should().Be(1);
        registro.VlBcCofins.Should().Be(950m);
        registro.AliqCofins.Should().Be(3m);
        registro.QuantBcCofins.Should().Be(10m);
        registro.AliqCofinsQuant.Should().Be(0.03m);
        registro.VlCofins.Should().Be(28.5m);
        registro.CodCta.Should().Be("3.1.01.001");
        registro.InfoCompl.Should().Be("Info Complementar");
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("C175".AsSpan(), out var meta);
        var registro = (RegistroC175)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlDesc
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);  // CstPis
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlBcPis
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqPis
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);  // QuantBcPis
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqPisQuant
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlPis
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcCofins
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqCofins
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty); // QuantBcCofins
        meta.Campos[13].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqCofinsQuant
        meta.Campos[14].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCofins
        meta.Campos[15].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta
        meta.Campos[16].Definidor(registro, ReadOnlySpan<char>.Empty); // InfoCompl

        registro.VlDesc.Should().BeNull();
        registro.CstPis.Should().BeNull();
        registro.VlBcPis.Should().BeNull();
        registro.AliqPis.Should().BeNull();
        registro.QuantBcPis.Should().BeNull();
        registro.AliqPisQuant.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlBcCofins.Should().BeNull();
        registro.AliqCofins.Should().BeNull();
        registro.QuantBcCofins.Should().BeNull();
        registro.AliqCofinsQuant.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CodCta.Should().BeNull();
        registro.InfoCompl.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // 18 campos → 19 separadores: |C175|Cfop|VlOpr|VlDesc|CstPis|VlBcPis|AliqPis|QuantBcPis|AliqPisQuant|VlPis|CstCofins|VlBcCofins|AliqCofins|QuantBcCofins|AliqCofinsQuant|VlCofins|CodCta|InfoCompl|
        const string sped =
            "|C175|5102|1000,00|50,00|1|950,00|0,6500|10,000|0,0100|6,18|1|950,00|3,0000|10,000|0,0300|28,50|3.1.01.001|Info Complementar|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // CstCofins=1, demais opcionais vazios: 8 pipes antes de "1", 8 pipes após "1"
        const string sped =
            "|C175|5102|1000,00||||||||1||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComAliquotaQuantidade_PreservaTextoCanonico()
    {
        // CstPis=3, QuantBcPis+AliqPisQuant preenchidos (VlBcPis/AliqPis vazios)
        const string sped =
            "|C175|5401|500,00||3|||100,000|0,0150|1,50|3|||100,000|0,0700|7,00|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

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
}

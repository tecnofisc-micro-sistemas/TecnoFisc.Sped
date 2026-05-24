using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC880Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC880).Assembly);

    [Fact]
    public void Atributo_DeclaraC880_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC880).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C880");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC880ComTrezeCamposNaOrdem()
    {
        _catalogo.TentarObter("C880".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C880");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodItem", "Cfop", "VlItem", "VlDesc",
            "CstPis", "QuantBcPis", "AliqPisQuant", "VlPis",
            "CstCofins", "QuantBcCofins", "AliqCofinsQuant", "VlCofins", "CodCta",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14]);
        meta.Campos[0].Tamanho.Should().Be(60);
        meta.Campos[0].Obrigatorio.Should().BeFalse();  // CodItem
        meta.Campos[1].Tamanho.Should().Be(4);
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // Cfop
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // VlItem
        meta.Campos[3].Obrigatorio.Should().BeFalse();  // VlDesc
        meta.Campos[4].Tamanho.Should().Be(2);
        meta.Campos[4].Obrigatorio.Should().BeTrue();   // CstPis
        meta.Campos[5].Obrigatorio.Should().BeFalse();  // QuantBcPis
        meta.Campos[6].Obrigatorio.Should().BeFalse();  // AliqPisQuant
        meta.Campos[8].Tamanho.Should().Be(2);
        meta.Campos[8].Obrigatorio.Should().BeTrue();   // CstCofins
        meta.Campos[12].Tamanho.Should().Be(255);
        meta.Campos[12].Obrigatorio.Should().BeFalse(); // CodCta
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C880".AsSpan(), out var meta);
        var registro = (RegistroC880)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "PROD001".AsSpan());       // CodItem
        meta.Campos[1].Definidor(registro, "5102".AsSpan());          // Cfop
        meta.Campos[2].Definidor(registro, "1000,00".AsSpan());       // VlItem
        meta.Campos[3].Definidor(registro, "50,00".AsSpan());         // VlDesc
        meta.Campos[4].Definidor(registro, "03".AsSpan());            // CstPis
        meta.Campos[5].Definidor(registro, "500,000".AsSpan());       // QuantBcPis
        meta.Campos[6].Definidor(registro, "0,0100".AsSpan());        // AliqPisQuant
        meta.Campos[7].Definidor(registro, "5,00".AsSpan());          // VlPis
        meta.Campos[8].Definidor(registro, "03".AsSpan());            // CstCofins
        meta.Campos[9].Definidor(registro, "500,000".AsSpan());       // QuantBcCofins
        meta.Campos[10].Definidor(registro, "0,0300".AsSpan());       // AliqCofinsQuant
        meta.Campos[11].Definidor(registro, "15,00".AsSpan());        // VlCofins
        meta.Campos[12].Definidor(registro, "3.1.01.001".AsSpan());   // CodCta

        registro.CodItem.Should().Be("PROD001");
        registro.Cfop.Should().Be(Cfop.Create("5102"));
        registro.VlItem.Should().Be(1000.00m);
        registro.VlDesc.Should().Be(50.00m);
        registro.CstPis.Should().Be(3);
        registro.QuantBcPis.Should().Be(500.000m);
        registro.AliqPisQuant.Should().Be(0.0100m);
        registro.VlPis.Should().Be(5.00m);
        registro.CstCofins.Should().Be(3);
        registro.QuantBcCofins.Should().Be(500.000m);
        registro.AliqCofinsQuant.Should().Be(0.0300m);
        registro.VlCofins.Should().Be(15.00m);
        registro.CodCta.Should().Be("3.1.01.001");
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("C880".AsSpan(), out var meta);
        var registro = (RegistroC880)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);   // CodItem
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);   // VlDesc
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);   // QuantBcPis
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);   // AliqPisQuant
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);   // VlPis
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);   // QuantBcCofins
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqCofinsQuant
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlCofins
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodCta

        registro.CodItem.Should().BeNull();
        registro.VlDesc.Should().BeNull();
        registro.QuantBcPis.Should().BeNull();
        registro.AliqPisQuant.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.QuantBcCofins.Should().BeNull();
        registro.AliqCofinsQuant.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C880|PROD001|5102|1000,00|50,00|49|500,000|0,0100|5,00|49|500,000|0,0300|15,00|3.1.01.001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposObrigatoriosApenas_PreservaTextoCanonico()
    {
        const string sped = "|C880||5102|1000,00||49||||49|||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CstOutrasOperacoes_PreservaTextoCanonico()
    {
        const string sped = "|C880||5102|800,00||99||||99|||||\r\n";

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

using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC396Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC396).Assembly);

    [Fact]
    public void Atributo_DeclaraC396_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC396).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C396");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC396ComTrezeCamposNaOrdem()
    {
        _catalogo.TentarObter("C396".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C396");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodItem", "VlItem", "VlDesc", "NatBcCred",
            "CstPis", "VlBcPis", "AliqPis", "VlPis",
            "CstCofins", "VlBcCofins", "AliqCofins", "VlCofins", "CodCta",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14]);
        meta.Campos[0].Tamanho.Should().Be(60);
        meta.Campos[0].Obrigatorio.Should().BeTrue();    // CodItem
        meta.Campos[1].Obrigatorio.Should().BeTrue();    // VlItem
        meta.Campos[2].Obrigatorio.Should().BeFalse();   // VlDesc
        meta.Campos[3].Tamanho.Should().Be(2);
        meta.Campos[3].Obrigatorio.Should().BeTrue();    // NatBcCred
        meta.Campos[4].Tamanho.Should().Be(2);
        meta.Campos[4].Obrigatorio.Should().BeTrue();    // CstPis
        meta.Campos[6].Tamanho.Should().Be(8);            // AliqPis tamanho fixo
        meta.Campos[8].Tamanho.Should().Be(2);
        meta.Campos[8].Obrigatorio.Should().BeTrue();    // CstCofins
        meta.Campos[10].Tamanho.Should().Be(8);          // AliqCofins tamanho fixo
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C396".AsSpan(), out var meta);
        var registro = (RegistroC396)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "PROD001".AsSpan());    // CodItem
        meta.Campos[1].Definidor(registro, "2000,00".AsSpan());    // VlItem
        meta.Campos[2].Definidor(registro, "50,00".AsSpan());      // VlDesc
        meta.Campos[3].Definidor(registro, "2".AsSpan());          // NatBcCred
        meta.Campos[4].Definidor(registro, "1".AsSpan());          // CstPis
        meta.Campos[5].Definidor(registro, "1950,00".AsSpan());    // VlBcPis
        meta.Campos[6].Definidor(registro, "1,6500".AsSpan());     // AliqPis
        meta.Campos[7].Definidor(registro, "32,18".AsSpan());      // VlPis
        meta.Campos[8].Definidor(registro, "1".AsSpan());          // CstCofins
        meta.Campos[9].Definidor(registro, "1950,00".AsSpan());    // VlBcCofins
        meta.Campos[10].Definidor(registro, "7,6000".AsSpan());    // AliqCofins
        meta.Campos[11].Definidor(registro, "148,20".AsSpan());    // VlCofins
        meta.Campos[12].Definidor(registro, "3.1.01.001".AsSpan()); // CodCta

        registro.CodItem.Should().Be("PROD001");
        registro.VlItem.Should().Be(2000m);
        registro.VlDesc.Should().Be(50m);
        registro.NatBcCred.Should().Be(CodigoBaseCalculoCredito.AquisicaoBensInsumo);
        registro.CstPis.Should().Be(1);
        registro.VlBcPis.Should().Be(1950m);
        registro.AliqPis.Should().Be(1.65m);
        registro.VlPis.Should().Be(32.18m);
        registro.CstCofins.Should().Be(1);
        registro.VlBcCofins.Should().Be(1950m);
        registro.AliqCofins.Should().Be(7.6m);
        registro.VlCofins.Should().Be(148.20m);
        registro.CodCta.Should().Be("3.1.01.001");
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("C396".AsSpan(), out var meta);
        var registro = (RegistroC396)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlDesc
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlBcPis
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqPis
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlPis
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlBcCofins
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqCofins
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCofins
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta

        registro.VlDesc.Should().BeNull();
        registro.VlBcPis.Should().BeNull();
        registro.AliqPis.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlBcCofins.Should().BeNull();
        registro.AliqCofins.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C396|PROD001|2000,00|50,00|02|1|1950,00|1,6500|32,18|1|1950,00|7,6000|148,20|3.1.01.001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|C396|INSUMO01|500,00||01|1|||15,00|1|||38,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComAliquotaQuantidade_PreservaTextoCanonico()
    {
        const string sped = "|C396|COMB001|3000,00||03|3||2,0000|6,00|3||2,0000|6,00||\r\n";

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

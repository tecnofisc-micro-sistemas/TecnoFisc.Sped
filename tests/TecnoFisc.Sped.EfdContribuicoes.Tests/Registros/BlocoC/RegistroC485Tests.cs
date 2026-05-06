using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC485Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC485).Assembly);

    [Fact]
    public void Atributo_DeclaraC485_Nivel5_BlocoC()
    {
        var atributo = typeof(RegistroC485).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C485");
        atributo.Nivel.Should().Be(5);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC485ComNoveCamposNaOrdem()
    {
        _catalogo.TentarObter("C485".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C485");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["CstCofins", "VlItem", "VlBcCofins", "AliqCofins", "QuantBcCofins", "AliqCofinsQuant", "VlCofins", "CodItem", "CodCta"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();    // CstCofins
        meta.Campos[1].Obrigatorio.Should().BeTrue();    // VlItem
        meta.Campos[2].Obrigatorio.Should().BeFalse();   // VlBcCofins
        meta.Campos[3].Tamanho.Should().Be(8);
        meta.Campos[3].Obrigatorio.Should().BeFalse();   // AliqCofins
        meta.Campos[6].Obrigatorio.Should().BeFalse();   // VlCofins
        meta.Campos[7].Tamanho.Should().Be(60);
        meta.Campos[7].Obrigatorio.Should().BeFalse();   // CodItem
        meta.Campos[8].Tamanho.Should().Be(255);
        meta.Campos[8].Obrigatorio.Should().BeFalse();   // CodCta
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C485".AsSpan(), out var meta);
        var registro = (RegistroC485)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01".AsSpan());          // CstCofins
        meta.Campos[1].Definidor(registro, "1500,00".AsSpan());     // VlItem
        meta.Campos[2].Definidor(registro, "1200,00".AsSpan());     // VlBcCofins
        meta.Campos[3].Definidor(registro, "7,6000".AsSpan());      // AliqCofins
        meta.Campos[4].Definidor(registro, "0,000".AsSpan());       // QuantBcCofins
        meta.Campos[5].Definidor(registro, "0,0000".AsSpan());      // AliqCofinsQuant
        meta.Campos[6].Definidor(registro, "76,00".AsSpan());       // VlCofins
        meta.Campos[7].Definidor(registro, "ITEM001".AsSpan());     // CodItem
        meta.Campos[8].Definidor(registro, "CONTA001".AsSpan());    // CodCta

        registro.CstCofins.Should().Be(1);
        registro.VlItem.Should().Be(1500m);
        registro.VlBcCofins.Should().Be(1200m);
        registro.AliqCofins.Should().Be(7.6000m);
        registro.QuantBcCofins.Should().Be(0m);
        registro.AliqCofinsQuant.Should().Be(0m);
        registro.VlCofins.Should().Be(76m);
        registro.CodItem.Should().Be("ITEM001");
        registro.CodCta.Should().Be("CONTA001");
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("C485".AsSpan(), out var meta);
        var registro = (RegistroC485)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcCofins
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqCofins
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // QuantBcCofins
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqCofinsQuant
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCofins
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty); // CodItem
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta

        registro.VlBcCofins.Should().BeNull();
        registro.AliqCofins.Should().BeNull();
        registro.QuantBcCofins.Should().BeNull();
        registro.AliqCofinsQuant.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CodItem.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C485|1|1500,00|1200,00|7,6000|0,000|0,0000|76,00|ITEM001|CONTA001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|C485|7|2000,00||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

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
}

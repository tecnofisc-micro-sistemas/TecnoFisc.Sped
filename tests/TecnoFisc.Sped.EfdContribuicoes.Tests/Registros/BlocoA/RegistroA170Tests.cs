using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoA;

public sealed class RegistroA170Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroA170).Assembly);

    [Fact]
    public void Atributo_DeclaraCodigoA170_Nivel4_BlocoA()
    {
        var atributo = typeof(RegistroA170).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("A170");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("A");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroA170Com17CamposNaOrdem()
    {
        _catalogo.TentarObter("A170".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("A170");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "NumItem", "CodItem", "DescrCompl", "VlItem", "VlDesc",
            "NatBcCred", "IndOrigCred", "CstPis",
            "VlBcPis", "AliqPis", "VlPis",
            "CstCofins", "VlBcCofins", "AliqCofins", "VlCofins",
            "CodCta", "CodCcus",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("A170".AsSpan(), out var meta);
        var registro = (RegistroA170)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());          // NumItem
        meta.Campos[1].Definidor(registro, "ITEM001".AsSpan());    // CodItem
        meta.Campos[2].Definidor(registro, "Servico TI".AsSpan()); // DescrCompl
        meta.Campos[3].Definidor(registro, "5000.00".AsSpan());    // VlItem
        meta.Campos[4].Definidor(registro, "100.00".AsSpan());     // VlDesc
        meta.Campos[5].Definidor(registro, "01".AsSpan());         // NatBcCred
        meta.Campos[6].Definidor(registro, "1".AsSpan());          // IndOrigCred
        meta.Campos[7].Definidor(registro, "49".AsSpan());         // CstPis
        meta.Campos[8].Definidor(registro, "5000.00".AsSpan());    // VlBcPis
        meta.Campos[9].Definidor(registro, "0.65".AsSpan());       // AliqPis
        meta.Campos[10].Definidor(registro, "32.50".AsSpan());     // VlPis
        meta.Campos[11].Definidor(registro, "49".AsSpan());        // CstCofins
        meta.Campos[12].Definidor(registro, "5000.00".AsSpan());   // VlBcCofins
        meta.Campos[13].Definidor(registro, "3.00".AsSpan());      // AliqCofins
        meta.Campos[14].Definidor(registro, "150.00".AsSpan());    // VlCofins
        meta.Campos[15].Definidor(registro, "1.01.001".AsSpan());  // CodCta
        meta.Campos[16].Definidor(registro, "CCUSTO01".AsSpan()); // CodCcus

        registro.NumItem.Should().Be(1);
        registro.CodItem.Should().Be("ITEM001");
        registro.DescrCompl.Should().Be("Servico TI");
        registro.VlItem.Should().Be(5000.00m);
        registro.VlDesc.Should().Be(100.00m);
        registro.NatBcCred.Should().Be("01");
        registro.IndOrigCred.Should().Be(IndicadorOrigemCredito.Importacao);
        registro.CstPis.Should().Be(49);
        registro.VlBcPis.Should().Be(5000.00m);
        registro.AliqPis.Should().Be(0.65m);
        registro.VlPis.Should().Be(32.50m);
        registro.CstCofins.Should().Be(49);
        registro.VlBcCofins.Should().Be(5000.00m);
        registro.AliqCofins.Should().Be(3.00m);
        registro.VlCofins.Should().Be(150.00m);
        registro.CodCta.Should().Be("1.01.001");
        registro.CodCcus.Should().Be("CCUSTO01");
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("A170".AsSpan(), out var meta);
        var registro = (RegistroA170)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);  // DescrCompl
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlDesc
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);  // NatBcCred
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);  // IndOrigCred
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlBcPis
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqPis
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty); // VlPis
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcCofins
        meta.Campos[13].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqCofins
        meta.Campos[14].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCofins
        meta.Campos[15].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta
        meta.Campos[16].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCcus

        registro.DescrCompl.Should().BeNull();
        registro.VlDesc.Should().BeNull();
        registro.NatBcCred.Should().BeNull();
        registro.IndOrigCred.Should().BeNull();
        registro.VlBcPis.Should().BeNull();
        registro.AliqPis.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlBcCofins.Should().BeNull();
        registro.AliqCofins.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CodCta.Should().BeNull();
        registro.CodCcus.Should().BeNull();
    }

    [Theory]
    [InlineData(IndicadorOrigemCredito.MercadoInterno, "0")]
    [InlineData(IndicadorOrigemCredito.Importacao, "1")]
    public void Serializar_IndOrigCred_RetornaCodigoSpedCorreto(
        IndicadorOrigemCredito origem, string esperado)
    {
        _catalogo.TentarObter("A170".AsSpan(), out var meta);
        var registro = (RegistroA170)meta!.Fabrica();
        registro.IndOrigCred = origem;

        meta.Campos[6].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public void Serializar_IndOrigCredNulo_DevolveVazio()
    {
        _catalogo.TentarObter("A170".AsSpan(), out var meta);
        var registro = (RegistroA170)meta!.Fabrica();
        registro.IndOrigCred = null;

        meta.Campos[6].Serializar(registro).Should().BeEmpty();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|A170|1|ITEM001|Servico TI|5000,00|100,00|01|1|49|5000,00|0,65|32,50|49|5000,00|3,00|150,00|1.01.001|CCUSTO01|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|A170|1|ITEM001||5000,00||||49||||49||||||\r\n";

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

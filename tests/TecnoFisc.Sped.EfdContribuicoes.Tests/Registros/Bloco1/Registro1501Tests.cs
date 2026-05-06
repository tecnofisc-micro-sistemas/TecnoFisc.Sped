using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco1;

public sealed class Registro1501Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1501).Assembly);

    [Fact]
    public void Atributo_Declara1501_Nivel3_Bloco1()
    {
        var atributo = typeof(Registro1501).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1501");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1501Com21CamposNaOrdem()
    {
        _catalogo.TentarObter("1501".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1501");
        meta.Campos.Select(c => c.Ordem).Should().Equal([
            2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22,
        ]);
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodPart", "CodItem", "CodMod", "Ser", "SubSer", "NumDoc",
            "DtOper", "ChvNfe",
            "VlOper", "Cfop", "NatBcCred", "IndOrigCred", "CstCofins",
            "VlBcCofins", "AliqCofins", "VlCofins",
            "CodCta", "CodCcus", "DescCompl", "PerEscrit",
            "Cnpj",
        ]);
        meta.Campos[0].Tamanho.Should().Be(60);
        meta.Campos[0].Obrigatorio.Should().BeFalse();  // CodPart
        meta.Campos[6].Tamanho.Should().Be(8);
        meta.Campos[6].Obrigatorio.Should().BeTrue();   // DtOper
        meta.Campos[8].Obrigatorio.Should().BeTrue();   // VlOper
        meta.Campos[10].Tamanho.Should().Be(2);
        meta.Campos[10].Obrigatorio.Should().BeTrue();  // NatBcCred
        meta.Campos[11].Obrigatorio.Should().BeTrue();  // IndOrigCred
        meta.Campos[12].Obrigatorio.Should().BeTrue();  // CstCofins
        meta.Campos[13].Obrigatorio.Should().BeTrue();  // VlBcCofins
        meta.Campos[14].Obrigatorio.Should().BeTrue();  // AliqCofins
        meta.Campos[15].Obrigatorio.Should().BeTrue();  // VlCofins
        meta.Campos[20].Tamanho.Should().Be(14);
        meta.Campos[20].Obrigatorio.Should().BeTrue();  // Cnpj
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1501".AsSpan(), out var meta);
        var registro = (Registro1501)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "PART001".AsSpan());                            // CodPart
        meta.Campos[1].Definidor(registro, "ITEM001".AsSpan());                            // CodItem
        meta.Campos[2].Definidor(registro, "55".AsSpan());                                 // CodMod
        meta.Campos[3].Definidor(registro, "001".AsSpan());                                // Ser
        meta.Campos[4].Definidor(registro, "SB1".AsSpan());                                // SubSer
        meta.Campos[5].Definidor(registro, "123456789".AsSpan());                          // NumDoc
        meta.Campos[6].Definidor(registro, "01012021".AsSpan());                           // DtOper
        meta.Campos[7].Definidor(registro, "35240111222333000181550010000000011000000018".AsSpan()); // ChvNfe
        meta.Campos[8].Definidor(registro, "1500,00".AsSpan());                            // VlOper
        meta.Campos[9].Definidor(registro, "5102".AsSpan());                               // Cfop
        meta.Campos[10].Definidor(registro, "01".AsSpan());                                // NatBcCred
        meta.Campos[11].Definidor(registro, "0".AsSpan());                                 // IndOrigCred
        meta.Campos[12].Definidor(registro, "50".AsSpan());                                // CstCofins
        meta.Campos[13].Definidor(registro, "1000,000".AsSpan());                          // VlBcCofins
        meta.Campos[14].Definidor(registro, "3,0000".AsSpan());                            // AliqCofins
        meta.Campos[15].Definidor(registro, "30,00".AsSpan());                             // VlCofins
        meta.Campos[16].Definidor(registro, "CONTA001".AsSpan());                          // CodCta
        meta.Campos[17].Definidor(registro, "CCUS001".AsSpan());                           // CodCcus
        meta.Campos[18].Definidor(registro, "Complementar".AsSpan());                     // DescCompl
        meta.Campos[19].Definidor(registro, "012021".AsSpan());                            // PerEscrit
        meta.Campos[20].Definidor(registro, "11222333000181".AsSpan());                    // Cnpj

        registro.CodPart.Should().Be("PART001");
        registro.CodItem.Should().Be("ITEM001");
        registro.CodMod.Should().Be("55");
        registro.Ser.Should().Be("001");
        registro.SubSer.Should().Be("SB1");
        registro.NumDoc.Should().Be(123456789);
        registro.DtOper.Should().Be(new DateOnly(2021, 1, 1));
        registro.ChvNfe.Should().NotBeNull();
        registro.ChvNfe!.Value.ToString().Should().Be("35240111222333000181550010000000011000000018");
        registro.VlOper.Should().Be(1500.00m);
        registro.Cfop.Should().NotBeNull();
        registro.Cfop!.Value.ToString().Should().Be("5102");
        registro.NatBcCred.Should().Be("01");
        registro.IndOrigCred.Should().Be(IndicadorOrigemCredito.MercadoInterno);
        registro.CstCofins.Should().Be(50);
        registro.VlBcCofins.Should().Be(1000.000m);
        registro.AliqCofins.Should().Be(3.0000m);
        registro.VlCofins.Should().Be(30.00m);
        registro.CodCta.Should().Be("CONTA001");
        registro.CodCcus.Should().Be("CCUS001");
        registro.DescCompl.Should().Be("Complementar");
        registro.PerEscrit.Should().Be("012021");
        registro.Cnpj.ToString().Should().Be("11222333000181");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1501".AsSpan(), out var meta);
        var registro = (Registro1501)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);   // CodPart
        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);   // CodItem
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);   // CodMod
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);   // Ser
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);   // SubSer
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);   // NumDoc
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);   // ChvNfe
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);   // Cfop
        meta.Campos[16].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodCta
        meta.Campos[17].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodCcus
        meta.Campos[18].Definidor(registro, ReadOnlySpan<char>.Empty);  // DescCompl
        meta.Campos[19].Definidor(registro, ReadOnlySpan<char>.Empty);  // PerEscrit

        registro.CodPart.Should().BeNull();
        registro.CodItem.Should().BeNull();
        registro.CodMod.Should().BeNull();
        registro.Ser.Should().BeNull();
        registro.SubSer.Should().BeNull();
        registro.NumDoc.Should().BeNull();
        registro.ChvNfe.Should().BeNull();
        registro.Cfop.Should().BeNull();
        registro.CodCta.Should().BeNull();
        registro.CodCcus.Should().BeNull();
        registro.DescCompl.Should().BeNull();
        registro.PerEscrit.Should().BeNull();
    }

    [Theory]
    [InlineData("0", IndicadorOrigemCredito.MercadoInterno)]
    [InlineData("1", IndicadorOrigemCredito.Importacao)]
    public void Definidor_IndOrigCred_AtribuiEnumCorreto(string codigo, IndicadorOrigemCredito esperado)
    {
        _catalogo.TentarObter("1501".AsSpan(), out var meta);
        var registro = (Registro1501)meta!.Fabrica();

        meta.Campos[11].Definidor(registro, codigo.AsSpan());

        registro.IndOrigCred.Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorOrigemCredito.MercadoInterno, "0")]
    [InlineData(IndicadorOrigemCredito.Importacao, "1")]
    public void Serializar_IndOrigCred_RetornaCodigoSpedCorreto(IndicadorOrigemCredito ind, string esperado)
    {
        _catalogo.TentarObter("1501".AsSpan(), out var meta);
        var registro = (Registro1501)meta!.Fabrica();
        registro.IndOrigCred = ind;

        meta.Campos[11].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1501|PART001|ITEM001|55|001|SB1|123456789|01012021|35240111222333000181550010000000011000000018|1500,00|5102|01|0|50|1000,000|3,0000|30,00|CONTA001|CCUS001|Complementar|012021|11222333000181|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SomenteObrigatorios_PreservaTextoCanonico()
    {
        const string sped = "|1501|||||||01012021||500,00||01|0|50|500,000|3,0000|15,00|||||11222333000181|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_OperacaoImportacao_PreservaTextoCanonico()
    {
        const string sped = "|1501|PART002||||||15012021||2000,00||03|1|50|2000,000|3,0000|60,00||||012021|11222333000181|\r\n";

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

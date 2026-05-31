using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco1;

public sealed class Registro1100Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1100).Assembly);

    [Fact]
    public void Atributo_Declara1100_Nivel2_Bloco1()
    {
        var atributo = typeof(Registro1100).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1100");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1100Com17CamposNaOrdem()
    {
        _catalogo.TentarObter("1100".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1100");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18]);
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "PerApuCred", "OrigCred", "CnpjSuc", "CodCred",
            "VlCredApu", "VlCredExtApu", "VlTotCredApu",
            "VlCredDescPaAnt", "VlCredPerPaAnt", "VlCredDcompPaAnt",
            "SdCredDispEfd",
            "VlCredDescEfd", "VlCredPerEfd", "VlCredDcompEfd",
            "VlCredTrans", "VlCredOut",
            "SldCredFim",
        ]);
        meta.Campos[0].Tamanho.Should().Be(6);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // PerApuCred
        meta.Campos[1].Tamanho.Should().Be(2);
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // OrigCred
        meta.Campos[2].Tamanho.Should().Be(14);
        meta.Campos[2].Obrigatorio.Should().BeFalse();  // CnpjSuc
        meta.Campos[3].Tamanho.Should().Be(3);
        meta.Campos[3].Obrigatorio.Should().BeTrue();   // CodCred
        meta.Campos[4].Obrigatorio.Should().BeTrue();   // VlCredApu
        meta.Campos[6].Obrigatorio.Should().BeTrue();   // VlTotCredApu
        meta.Campos[7].Obrigatorio.Should().BeTrue();   // VlCredDescPaAnt
        meta.Campos[10].Obrigatorio.Should().BeTrue();  // SdCredDispEfd
        meta.Campos[16].Obrigatorio.Should().BeTrue();  // SldCredFim
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1100".AsSpan(), out var meta);
        var registro = (Registro1100)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "012021".AsSpan());        // PerApuCred
        meta.Campos[1].Definidor(registro, "01".AsSpan());            // OrigCred
        meta.Campos[2].Definidor(registro, "11222333000181".AsSpan()); // CnpjSuc
        meta.Campos[3].Definidor(registro, "101".AsSpan());           // CodCred
        meta.Campos[4].Definidor(registro, "1000,00".AsSpan());       // VlCredApu
        meta.Campos[5].Definidor(registro, "200,00".AsSpan());        // VlCredExtApu
        meta.Campos[6].Definidor(registro, "1200,00".AsSpan());       // VlTotCredApu
        meta.Campos[7].Definidor(registro, "100,00".AsSpan());        // VlCredDescPaAnt
        meta.Campos[8].Definidor(registro, "50,00".AsSpan());         // VlCredPerPaAnt
        meta.Campos[9].Definidor(registro, "30,00".AsSpan());         // VlCredDcompPaAnt
        meta.Campos[10].Definidor(registro, "1020,00".AsSpan());      // SdCredDispEfd
        meta.Campos[11].Definidor(registro, "400,00".AsSpan());       // VlCredDescEfd
        meta.Campos[12].Definidor(registro, "200,00".AsSpan());       // VlCredPerEfd
        meta.Campos[13].Definidor(registro, "100,00".AsSpan());       // VlCredDcompEfd
        meta.Campos[14].Definidor(registro, "50,00".AsSpan());        // VlCredTrans
        meta.Campos[15].Definidor(registro, "70,00".AsSpan());        // VlCredOut
        meta.Campos[16].Definidor(registro, "200,00".AsSpan());       // SldCredFim

        registro.PerApuCred.Should().Be("012021");
        registro.OrigCred.Should().Be(IndicadorOrigemCreditoFiscal.Proprias);
        registro.CnpjSuc.Should().NotBeNull();
        registro.CnpjSuc!.Value.ToString().Should().Be("11222333000181");
        registro.CodCred.Should().Be(CodigoTipoCredito.MercadoInternoTributadoAliquotaBasica);
        registro.VlCredApu.Should().Be(1000.00m);
        registro.VlCredExtApu.Should().Be(200.00m);
        registro.VlTotCredApu.Should().Be(1200.00m);
        registro.VlCredDescPaAnt.Should().Be(100.00m);
        registro.VlCredPerPaAnt.Should().Be(50.00m);
        registro.VlCredDcompPaAnt.Should().Be(30.00m);
        registro.SdCredDispEfd.Should().Be(1020.00m);
        registro.VlCredDescEfd.Should().Be(400.00m);
        registro.VlCredPerEfd.Should().Be(200.00m);
        registro.VlCredDcompEfd.Should().Be(100.00m);
        registro.VlCredTrans.Should().Be(50.00m);
        registro.VlCredOut.Should().Be(70.00m);
        registro.SldCredFim.Should().Be(200.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1100".AsSpan(), out var meta);
        var registro = (Registro1100)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);  // CnpjSuc
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlCredExtApu
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlCredPerPaAnt
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlCredDcompPaAnt
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCredDescEfd
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCredPerEfd
        meta.Campos[13].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCredDcompEfd
        meta.Campos[14].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCredTrans
        meta.Campos[15].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCredOut

        registro.CnpjSuc.Should().BeNull();
        registro.VlCredExtApu.Should().BeNull();
        registro.VlCredPerPaAnt.Should().BeNull();
        registro.VlCredDcompPaAnt.Should().BeNull();
        registro.VlCredDescEfd.Should().BeNull();
        registro.VlCredPerEfd.Should().BeNull();
        registro.VlCredDcompEfd.Should().BeNull();
        registro.VlCredTrans.Should().BeNull();
        registro.VlCredOut.Should().BeNull();
    }

    [Theory]
    [InlineData("01", IndicadorOrigemCreditoFiscal.Proprias)]
    [InlineData("02", IndicadorOrigemCreditoFiscal.Sucedida)]
    public void Definidor_OrigCred_AtribuiEnumCorreto(string codigo, IndicadorOrigemCreditoFiscal esperado)
    {
        _catalogo.TentarObter("1100".AsSpan(), out var meta);
        var registro = (Registro1100)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, codigo.AsSpan());

        registro.OrigCred.Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorOrigemCreditoFiscal.Proprias, "01")]
    [InlineData(IndicadorOrigemCreditoFiscal.Sucedida, "02")]
    public void Serializar_OrigCred_RetornaCodigoSpedCorreto(IndicadorOrigemCreditoFiscal orig, string esperado)
    {
        _catalogo.TentarObter("1100".AsSpan(), out var meta);
        var registro = (Registro1100)meta!.Fabrica();
        registro.OrigCred = orig;

        meta.Campos[1].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1100|012021|01|11222333000181|101|1000,00|200,00|1200,00|100,00|50,00|30,00|1020,00|400,00|200,00|100,00|50,00|70,00|200,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CreditoProprioSemCnpjSuc_PreservaTextoCanonico()
    {
        const string sped = "|1100|032020|01||201|500,00||500,00|100,00|||400,00||||||400,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CreditoSucedido_PreservaTextoCanonico()
    {
        const string sped = "|1100|062019|02|11222333000181|301|2500,00||2500,00|500,00|||2000,00||||||2000,00|\r\n";

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

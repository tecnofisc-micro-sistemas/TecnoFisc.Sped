using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoF;

public sealed class RegistroF200Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroF200).Assembly);

    [Fact]
    public void Atributo_DeclaraF200_Nivel3_BlocoF()
    {
        var atributo = typeof(RegistroF200).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("F200");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("F");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroF200Com21CamposNaOrdem()
    {
        _catalogo.TentarObter("F200".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("F200");
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "IndOper", "UnidImob", "IdentEmp", "DescUnidImob", "NumCont",
            "CpfCnpjAdq", "DtOper", "VlTotVend", "VlRecAcum", "VlTotRec",
            "CstPis", "VlBcPis", "AliqPis", "VlPis",
            "CstCofins", "VlBcCofins", "AliqCofins", "VlCofins",
            "PercRecReceb", "IndNatEmp", "InfComp"
        ]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("F200".AsSpan(), out var meta);
        var registro = (RegistroF200)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01".AsSpan());                   // IndOper
        meta.Campos[1].Definidor(registro, "04".AsSpan());                   // UnidImob
        meta.Campos[2].Definidor(registro, "Empreendimento Alpha".AsSpan()); // IdentEmp
        meta.Campos[3].Definidor(registro, "Apto 201 Torre B".AsSpan());     // DescUnidImob
        meta.Campos[4].Definidor(registro, "CT-2024/0042".AsSpan());         // NumCont
        meta.Campos[5].Definidor(registro, "12345678901234".AsSpan());       // CpfCnpjAdq
        meta.Campos[6].Definidor(registro, "15032024".AsSpan());             // DtOper
        meta.Campos[7].Definidor(registro, "500000,00".AsSpan());            // VlTotVend
        meta.Campos[8].Definidor(registro, "200000,00".AsSpan());            // VlRecAcum
        meta.Campos[9].Definidor(registro, "50000,00".AsSpan());             // VlTotRec
        meta.Campos[10].Definidor(registro, "50".AsSpan());                  // CstPis
        meta.Campos[11].Definidor(registro, "325000,00".AsSpan());           // VlBcPis
        meta.Campos[12].Definidor(registro, "1,6500".AsSpan());              // AliqPis
        meta.Campos[13].Definidor(registro, "5362,50".AsSpan());             // VlPis
        meta.Campos[14].Definidor(registro, "50".AsSpan());                  // CstCofins
        meta.Campos[15].Definidor(registro, "325000,00".AsSpan());           // VlBcCofins
        meta.Campos[16].Definidor(registro, "7,6000".AsSpan());              // AliqCofins
        meta.Campos[17].Definidor(registro, "24700,00".AsSpan());            // VlCofins
        meta.Campos[18].Definidor(registro, "50,00".AsSpan());               // PercRecReceb
        meta.Campos[19].Definidor(registro, "3".AsSpan());                   // IndNatEmp
        meta.Campos[20].Definidor(registro, "Informacao complementar".AsSpan()); // InfComp

        registro.IndOper.Should().Be(IndicadorOperacaoImobiliaria.VendaVistaUnidadeConcluida);
        registro.UnidImob.Should().Be(IndicadorUnidadeImobiliaria.UnidadeIncorporacao);
        registro.IdentEmp.Should().Be("Empreendimento Alpha");
        registro.DescUnidImob.Should().Be("Apto 201 Torre B");
        registro.NumCont.Should().Be("CT-2024/0042");
        registro.CpfCnpjAdq.Should().Be("12345678901234");
        registro.DtOper.Should().Be(new DateOnly(2024, 3, 15));
        registro.VlTotVend.Should().Be(500000.00m);
        registro.VlRecAcum.Should().Be(200000.00m);
        registro.VlTotRec.Should().Be(50000.00m);
        registro.CstPis.Should().Be("50");
        registro.VlBcPis.Should().Be(325000.00m);
        registro.AliqPis.Should().Be(1.6500m);
        registro.VlPis.Should().Be(5362.50m);
        registro.CstCofins.Should().Be("50");
        registro.VlBcCofins.Should().Be(325000.00m);
        registro.AliqCofins.Should().Be(7.6000m);
        registro.VlCofins.Should().Be(24700.00m);
        registro.PercRecReceb.Should().Be(50.00m);
        registro.IndNatEmp.Should().Be(IndicadorNaturezaEmpreendimento.IncorporacaoCondominio);
        registro.InfComp.Should().Be("Informacao complementar");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("F200".AsSpan(), out var meta);
        var registro = (RegistroF200)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);  // DescUnidImob
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);  // NumCont
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlRecAcum
        meta.Campos[18].Definidor(registro, ReadOnlySpan<char>.Empty); // PercRecReceb
        meta.Campos[19].Definidor(registro, ReadOnlySpan<char>.Empty); // IndNatEmp
        meta.Campos[20].Definidor(registro, ReadOnlySpan<char>.Empty); // InfComp

        registro.DescUnidImob.Should().BeNull();
        registro.NumCont.Should().BeNull();
        registro.VlRecAcum.Should().BeNull();
        registro.PercRecReceb.Should().BeNull();
        registro.IndNatEmp.Should().BeNull();
        registro.InfComp.Should().BeNull();
    }

    [Theory]
    [InlineData("01", IndicadorOperacaoImobiliaria.VendaVistaUnidadeConcluida)]
    [InlineData("02", IndicadorOperacaoImobiliaria.VendaPrazoUnidadeConcluida)]
    [InlineData("03", IndicadorOperacaoImobiliaria.VendaVistaUnidadeConstrucao)]
    [InlineData("04", IndicadorOperacaoImobiliaria.VendaPrazoUnidadeConstrucao)]
    [InlineData("05", IndicadorOperacaoImobiliaria.Outras)]
    public void Definidor_IndOper_ConverteTodosOsValores(string texto, IndicadorOperacaoImobiliaria esperado)
    {
        _catalogo.TentarObter("F200".AsSpan(), out var meta);
        var registro = (RegistroF200)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, texto.AsSpan());

        registro.IndOper.Should().Be(esperado);
    }

    [Theory]
    [InlineData("01", IndicadorUnidadeImobiliaria.TerrenoAdquiridoParaVenda)]
    [InlineData("02", IndicadorUnidadeImobiliaria.TerrenoLoteamento)]
    [InlineData("03", IndicadorUnidadeImobiliaria.LoteDesmembramento)]
    [InlineData("04", IndicadorUnidadeImobiliaria.UnidadeIncorporacao)]
    [InlineData("05", IndicadorUnidadeImobiliaria.PredioVenda)]
    [InlineData("06", IndicadorUnidadeImobiliaria.Outras)]
    public void Definidor_UnidImob_ConverteTodosOsValores(string texto, IndicadorUnidadeImobiliaria esperado)
    {
        _catalogo.TentarObter("F200".AsSpan(), out var meta);
        var registro = (RegistroF200)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, texto.AsSpan());

        registro.UnidImob.Should().Be(esperado);
    }

    [Theory]
    [InlineData("1", IndicadorNaturezaEmpreendimento.Consorcio)]
    [InlineData("2", IndicadorNaturezaEmpreendimento.Scp)]
    [InlineData("3", IndicadorNaturezaEmpreendimento.IncorporacaoCondominio)]
    [InlineData("4", IndicadorNaturezaEmpreendimento.Outras)]
    [InlineData("", null)]
    public void Definidor_IndNatEmp_ConverteTodosOsValores(string texto, IndicadorNaturezaEmpreendimento? esperado)
    {
        _catalogo.TentarObter("F200".AsSpan(), out var meta);
        var registro = (RegistroF200)meta!.Fabrica();

        meta.Campos[19].Definidor(registro, texto.AsSpan());

        registro.IndNatEmp.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|F200|01|04|Empreendimento Alpha|Apto 201 Torre B|CT-2024/0042|12345678901234|15032024|500000,00|200000,00|50000,00|50|325000,00|1,6500|5362,50|50|325000,00|7,6000|24700,00|50,00|3|Informacao complementar|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        const string sped =
            "|F200|01|04|Empreendimento Beta|||12345678000195|01062024|800000,00||100000,00|50||||50|||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCpfAdquirente_PreservaTextoCanonico()
    {
        const string sped =
            "|F200|03|05|Edificio Horizonte|Sala 304||12345678901|20092024|1200000,00||80000,00|50||||50|||||||\r\n";

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

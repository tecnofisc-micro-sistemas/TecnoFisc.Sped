using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoF;

public sealed class RegistroF800Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroF800).Assembly);

    [Fact]
    public void Atributo_DeclaraF800_Nivel3_BlocoF()
    {
        var atributo = typeof(RegistroF800).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("F800");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("F");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroF800Com8CamposNaOrdem()
    {
        _catalogo.TentarObter("F800".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("F800");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "IndNatEven", "DtEven", "CnpjSuced", "PaContCred",
            "CodCred", "VlCredPis", "VlCredCofins", "PerCredCis",
        ]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();  // IndNatEven
        meta.Campos[1].Tamanho.Should().Be(8);
        meta.Campos[1].Obrigatorio.Should().BeTrue();  // DtEven
        meta.Campos[2].Tamanho.Should().Be(14);
        meta.Campos[2].Obrigatorio.Should().BeTrue();  // CnpjSuced
        meta.Campos[3].Tamanho.Should().Be(6);
        meta.Campos[3].Obrigatorio.Should().BeTrue();  // PaContCred
        meta.Campos[4].Tamanho.Should().Be(3);
        meta.Campos[4].Obrigatorio.Should().BeTrue();  // CodCred
        meta.Campos[5].Obrigatorio.Should().BeTrue();  // VlCredPis
        meta.Campos[6].Obrigatorio.Should().BeTrue();  // VlCredCofins
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("F800".AsSpan(), out var meta);
        var registro = (RegistroF800)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01".AsSpan());             // IndNatEven
        meta.Campos[1].Definidor(registro, "15062023".AsSpan());       // DtEven
        meta.Campos[2].Definidor(registro, "12345678000195".AsSpan()); // CnpjSuced
        meta.Campos[3].Definidor(registro, "012023".AsSpan());         // PaContCred
        meta.Campos[4].Definidor(registro, "101".AsSpan());            // CodCred
        meta.Campos[5].Definidor(registro, "10000,00".AsSpan());       // VlCredPis
        meta.Campos[6].Definidor(registro, "46000,00".AsSpan());       // VlCredCofins
        meta.Campos[7].Definidor(registro, "50,00".AsSpan());          // PerCredCis

        registro.IndNatEven.Should().Be(IndicadorNaturezaEvento.Incorporacao);
        registro.DtEven.Should().Be(new DateOnly(2023, 6, 15));
        registro.CnpjSuced.Should().Be(Cnpj.Create("12345678000195"));
        registro.PaContCred.Should().Be("012023");
        registro.CodCred.Should().Be(CodigoTipoCredito.MercadoInternoTributadoAliquotaBasica);
        registro.VlCredPis.Should().Be(10000m);
        registro.VlCredCofins.Should().Be(46000m);
        registro.PerCredCis.Should().Be(50m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("F800".AsSpan(), out var meta);
        var registro = (RegistroF800)meta!.Fabrica();

        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty); // PerCredCis

        registro.PerCredCis.Should().BeNull();
    }

    [Theory]
    [InlineData("01", IndicadorNaturezaEvento.Incorporacao)]
    [InlineData("02", IndicadorNaturezaEvento.Fusao)]
    [InlineData("03", IndicadorNaturezaEvento.CisaoTotal)]
    [InlineData("04", IndicadorNaturezaEvento.CisaoParcial)]
    [InlineData("99", IndicadorNaturezaEvento.Outros)]
    public void Definidor_IndNatEven_AtribuiEnumCorreto(string codigo, IndicadorNaturezaEvento esperado)
    {
        _catalogo.TentarObter("F800".AsSpan(), out var meta);
        var registro = (RegistroF800)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, codigo.AsSpan());

        registro.IndNatEven.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|F800|01|15062023|12345678000195|012023|101|10000,00|46000,00|50,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemPerCredCis_PreservaTextoCanonico()
    {
        const string sped =
            "|F800|02|01032022|11444777000161|062021|201|5000,00|23000,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CisaoParcial_PreservaTextoCanonico()
    {
        const string sped =
            "|F800|04|30092023|11222333000181|032023|301|3000,00|13800,00|40,00|\r\n";

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

using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.248 - exercita a forma do <see cref="Registro1970"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 298-299): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1970Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1970).Assembly);

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

    [Fact]
    public void Atributo_Declara1970_Nivel2_Bloco1()
    {
        var atributo = typeof(Registro1970).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1970");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1970Com11CamposNaOrdem()
    {
        _catalogo.TentarObter("1970".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1970");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndAp",
            "G301",
            "G302",
            "G303",
            "G304",
            "G305",
            "G306",
            "G307",
            "G3T",
            "G308",
            "G309",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 11));
        meta.Campos.Select(c => c.Tamanho).Should().Equal([2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
        meta.Campos.Select(c => c.Decimais).Should().Equal([0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2]);
        meta.Campos.Where(c => c.Obrigatorio).Select(c => c.Nome).Should().Equal([
            "IndAp",
            "G301",
            "G302",
            "G303",
            "G304",
            "G305",
            "G306",
            "G307",
            "G3T",
            "G308",
            "G309",
        ]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1970".AsSpan(), out var meta);
        var registro = (Registro1970)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "03".AsSpan());
        meta.Campos[1].Definidor(registro, "10000,00".AsSpan());
        meta.Campos[2].Definidor(registro, "1200,00".AsSpan());
        meta.Campos[3].Definidor(registro, "2500,00".AsSpan());
        meta.Campos[4].Definidor(registro, "12,50".AsSpan());
        meta.Campos[5].Definidor(registro, "4000,00".AsSpan());
        meta.Campos[6].Definidor(registro, "720,00".AsSpan());
        meta.Campos[7].Definidor(registro, "90,00".AsSpan());
        meta.Campos[8].Definidor(registro, "150,00".AsSpan());
        meta.Campos[9].Definidor(registro, "2000,00".AsSpan());
        meta.Campos[10].Definidor(registro, "1850,00".AsSpan());

        registro.IndAp.Should().Be(IndicadorSubApuracaoIcms.Apuracao1);
        registro.G301.Should().Be(10000.00m);
        registro.G302.Should().Be(1200.00m);
        registro.G303.Should().Be(2500.00m);
        registro.G304.Should().Be(12.50m);
        registro.G305.Should().Be(4000.00m);
        registro.G306.Should().Be(720.00m);
        registro.G307.Should().Be(90.00m);
        registro.G3T.Should().Be(150.00m);
        registro.G308.Should().Be(2000.00m);
        registro.G309.Should().Be(1850.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1970".AsSpan(), out var meta);
        var registro = (Registro1970)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);

        registro.IndAp.Should().BeNull();
    }

    [Theory]
    [InlineData("", null)]
    [InlineData("03", IndicadorSubApuracaoIcms.Apuracao1)]
    [InlineData("04", IndicadorSubApuracaoIcms.Apuracao2)]
    [InlineData("05", IndicadorSubApuracaoIcms.Apuracao3)]
    [InlineData("06", IndicadorSubApuracaoIcms.Apuracao4)]
    [InlineData("07", IndicadorSubApuracaoIcms.Apuracao5)]
    [InlineData("08", IndicadorSubApuracaoIcms.Apuracao6)]
    public void IndAp_Definidor_AtribuiValorCorreto(string valor, IndicadorSubApuracaoIcms? esperado)
    {
        _catalogo.TentarObter("1970".AsSpan(), out var meta);
        var registro = (Registro1970)meta!.Fabrica();

        meta!.Campos[0].Definidor(registro, valor.AsSpan());

        registro.IndAp.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|1970|03|10000,00|1200,00|2500,00|12,50|4000,00|720,00|90,00|150,00|2000,00|1850,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComApuracao6EValoresZerados_PreservaTextoCanonico()
    {
        const string sped = "|1970|08|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

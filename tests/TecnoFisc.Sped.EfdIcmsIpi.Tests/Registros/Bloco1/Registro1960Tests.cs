using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.247 - exercita a forma do <see cref="Registro1960"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 296-298): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1960Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1960).Assembly);

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
    public void Atributo_Declara1960_Nivel2_Bloco1()
    {
        var atributo = typeof(Registro1960).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1960");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1960Com12CamposNaOrdem()
    {
        _catalogo.TentarObter("1960".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1960");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndAp",
            "G101",
            "G102",
            "G103",
            "G104",
            "G105",
            "G106",
            "G107",
            "G108",
            "G109",
            "G110",
            "G111",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 12));
        meta.Campos.Select(c => c.Tamanho).Should().Equal([2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
        meta.Campos.Select(c => c.Decimais).Should().Equal([0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2]);
        meta.Campos.Where(c => c.Obrigatorio).Select(c => c.Nome).Should().Equal([
            "IndAp",
            "G101",
            "G102",
            "G103",
            "G104",
            "G105",
            "G106",
            "G107",
            "G108",
            "G109",
            "G110",
            "G111",
        ]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1960".AsSpan(), out var meta);
        var registro = (Registro1960)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "03".AsSpan());
        meta.Campos[1].Definidor(registro, "12,50".AsSpan());
        meta.Campos[2].Definidor(registro, "1000,00".AsSpan());
        meta.Campos[3].Definidor(registro, "2500,00".AsSpan());
        meta.Campos[4].Definidor(registro, "500,00".AsSpan());
        meta.Campos[5].Definidor(registro, "900,00".AsSpan());
        meta.Campos[6].Definidor(registro, "600,00".AsSpan());
        meta.Campos[7].Definidor(registro, "25,00".AsSpan());
        meta.Campos[8].Definidor(registro, "575,00".AsSpan());
        meta.Campos[9].Definidor(registro, "71,88".AsSpan());
        meta.Campos[10].Definidor(registro, "96,88".AsSpan());
        meta.Campos[11].Definidor(registro, "803,12".AsSpan());

        registro.IndAp.Should().Be(IndicadorSubApuracaoIcms.Apuracao1);
        registro.G101.Should().Be(12.50m);
        registro.G102.Should().Be(1000.00m);
        registro.G103.Should().Be(2500.00m);
        registro.G104.Should().Be(500.00m);
        registro.G105.Should().Be(900.00m);
        registro.G106.Should().Be(600.00m);
        registro.G107.Should().Be(25.00m);
        registro.G108.Should().Be(575.00m);
        registro.G109.Should().Be(71.88m);
        registro.G110.Should().Be(96.88m);
        registro.G111.Should().Be(803.12m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1960".AsSpan(), out var meta);
        var registro = (Registro1960)meta!.Fabrica();

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
        _catalogo.TentarObter("1960".AsSpan(), out var meta);
        var registro = (Registro1960)meta!.Fabrica();

        meta!.Campos[0].Definidor(registro, valor.AsSpan());

        registro.IndAp.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|1960|03|12,50|1000,00|2500,00|500,00|900,00|600,00|25,00|575,00|71,88|96,88|803,12|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComApuracao6EValoresZerados_PreservaTextoCanonico()
    {
        const string sped = "|1960|08|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

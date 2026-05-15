using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.250 - exercita a forma do <see cref="Registro1980"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 300-301): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1980Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1980).Assembly);

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

    [Fact]
    public void Atributo_Declara1980_Nivel2_Bloco1()
    {
        var atributo = typeof(Registro1980).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1980");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1980Com13CamposNaOrdem()
    {
        _catalogo.TentarObter("1980".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1980");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndAp",
            "G401",
            "G402",
            "G403",
            "G404",
            "G405",
            "G406",
            "G407",
            "G408",
            "G409",
            "G410",
            "G411",
            "G412",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 13));
        meta.Campos.Select(c => c.Tamanho).Should().Equal([2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
        meta.Campos.Select(c => c.Decimais).Should().Equal([0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2]);
        meta.Campos.Where(c => c.Obrigatorio).Select(c => c.Nome).Should().Equal([
            "IndAp",
            "G401",
            "G402",
            "G403",
            "G404",
            "G405",
            "G406",
            "G407",
            "G408",
            "G409",
            "G410",
            "G411",
            "G412",
        ]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1980".AsSpan(), out var meta);
        var registro = (Registro1980)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "02".AsSpan());
        meta.Campos[1].Definidor(registro, "3,50".AsSpan());
        meta.Campos[2].Definidor(registro, "1000,00".AsSpan());
        meta.Campos[3].Definidor(registro, "2000,00".AsSpan());
        meta.Campos[4].Definidor(registro, "2,50".AsSpan());
        meta.Campos[5].Definidor(registro, "1500,00".AsSpan());
        meta.Campos[6].Definidor(registro, "2500,00".AsSpan());
        meta.Campos[7].Definidor(registro, "900,00".AsSpan());
        meta.Campos[8].Definidor(registro, "70,00".AsSpan());
        meta.Campos[9].Definidor(registro, "62,50".AsSpan());
        meta.Campos[10].Definidor(registro, "132,50".AsSpan());
        meta.Campos[11].Definidor(registro, "767,50".AsSpan());
        meta.Campos[12].Definidor(registro, "85,28".AsSpan());

        registro.IndAp.Should().Be(IndicadorSubApuracaoIcms.CentralDistribuicao);
        registro.G401.Should().Be(3.50m);
        registro.G402.Should().Be(1000.00m);
        registro.G403.Should().Be(2000.00m);
        registro.G404.Should().Be(2.50m);
        registro.G405.Should().Be(1500.00m);
        registro.G406.Should().Be(2500.00m);
        registro.G407.Should().Be(900.00m);
        registro.G408.Should().Be(70.00m);
        registro.G409.Should().Be(62.50m);
        registro.G410.Should().Be(132.50m);
        registro.G411.Should().Be(767.50m);
        registro.G412.Should().Be(85.28m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1980".AsSpan(), out var meta);
        var registro = (Registro1980)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);

        registro.IndAp.Should().BeNull();
    }

    [Theory]
    [InlineData("", null)]
    [InlineData("02", IndicadorSubApuracaoIcms.CentralDistribuicao)]
    public void IndAp_Definidor_AtribuiValorCorreto(string valor, IndicadorSubApuracaoIcms? esperado)
    {
        _catalogo.TentarObter("1980".AsSpan(), out var meta);
        var registro = (Registro1980)meta!.Fabrica();

        meta!.Campos[0].Definidor(registro, valor.AsSpan());

        registro.IndAp.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|1980|02|3,50|1000,00|2000,00|2,50|1500,00|2500,00|900,00|70,00|62,50|132,50|767,50|85,28|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComValoresZerados_PreservaTextoCanonico()
    {
        const string sped = "|1980|02|3,00|0,00|0,00|2,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}

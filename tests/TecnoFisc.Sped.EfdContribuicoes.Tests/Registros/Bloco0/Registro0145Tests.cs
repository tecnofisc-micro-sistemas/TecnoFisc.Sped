using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco0;

public sealed class Registro0145Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0145).Assembly);

    [Fact]
    public void Atributo_DeclaraCodigo0145_Nivel3_Bloco0()
    {
        var atributo = typeof(Registro0145).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0145");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0145ComCincoCamposNaOrdem()
    {
        _catalogo.TentarObter("0145".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0145");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodIncTrib",
            "VlRecTot",
            "VlRecAtiv",
            "VlRecDemaisAtiv",
            "InfoCompl",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0145".AsSpan(), out var meta);
        var registro = (Registro0145)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());
        meta.Campos[1].Definidor(registro, "1000000,00".AsSpan());
        meta.Campos[2].Definidor(registro, "800000,00".AsSpan());
        meta.Campos[3].Definidor(registro, "200000,00".AsSpan());
        meta.Campos[4].Definidor(registro, "Informacao complementar teste".AsSpan());

        registro.CodIncTrib.Should().Be(CodigoIncidenciaContribPrevidenciaria.ReceitaBrutaExclusivamente);
        registro.VlRecTot.Should().Be(1000000.00m);
        registro.VlRecAtiv.Should().Be(800000.00m);
        registro.VlRecDemaisAtiv.Should().Be(200000.00m);
        registro.InfoCompl.Should().Be("Informacao complementar teste");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("0145".AsSpan(), out var meta);
        var registro = (Registro0145)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // VlRecDemaisAtiv
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // InfoCompl

        registro.VlRecDemaisAtiv.Should().BeNull();
        registro.InfoCompl.Should().BeNull();
    }

    [Theory]
    [InlineData(CodigoIncidenciaContribPrevidenciaria.ReceitaBrutaExclusivamente, "1")]
    [InlineData(CodigoIncidenciaContribPrevidenciaria.ReceitaBrutaERemuneracoes, "2")]
    public void Serializar_CodIncTrib_RetornaCodigoSped(CodigoIncidenciaContribPrevidenciaria valor, string esperado)
    {
        _catalogo.TentarObter("0145".AsSpan(), out var meta);
        var registro = (Registro0145)meta!.Fabrica();
        registro.CodIncTrib = valor;

        meta.Campos[0].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|0145|1|1000000,00|800000,00|200000,00|Complementar|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // Apenas campos obrigatórios — VlRecDemaisAtiv e InfoCompl vazios
        const string sped =
            "|0145|2|500000,00|500000,00|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_AninhadoSob0140_RespeitaHierarquia()
    {
        const string sped =
            "|0000|006|0|||01012025|31012025|EMPRESA CONTRIB SA|11222333000181|SP|3550308||00|2|\r\n" +
            "|0001|0|\r\n" +
            "|0140||EMPRESA CONTRIB SA|11222333000181|SP||3550308|||\r\n" +
            "|0145|1|2000000,00|2000000,00|||\r\n";

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

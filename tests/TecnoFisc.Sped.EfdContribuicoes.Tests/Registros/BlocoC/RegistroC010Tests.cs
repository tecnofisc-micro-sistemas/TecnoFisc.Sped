using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC010Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC010).Assembly);

    [Fact]
    public void Atributo_DeclaraCodigoC010_Nivel2_BlocoC()
    {
        var atributo = typeof(RegistroC010).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C010");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC010Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("C010".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C010");
        meta.Campos.Select(c => c.Nome).Should().Equal(["Cnpj", "IndEscri"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
        meta.Campos[0].Tamanho.Should().Be(14);
        meta.Campos[0].Obrigatorio.Should().BeTrue();
        meta.Campos[1].Tamanho.Should().Be(1);
        meta.Campos[1].Obrigatorio.Should().BeFalse();
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C010".AsSpan(), out var meta);
        var registro = (RegistroC010)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "11222333000181".AsSpan());
        meta.Campos[1].Definidor(registro, "1".AsSpan());

        registro.Cnpj.ToString().Should().Be("11222333000181");
        registro.IndEscri.Should().Be(IndicadorEscrituracaoNfe.ConsolidacaoPorNfe);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C010".AsSpan(), out var meta);
        var registro = (RegistroC010)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.IndEscri.Should().BeNull();
    }

    [Fact]
    public void Definidor_CampoCnpjVazio_SetaCnpjDefault()
    {
        // Cnpj é value type não anulável (Obrigatorio = true); catálogo retorna default(Cnpj).
        _catalogo.TentarObter("C010".AsSpan(), out var meta);
        var registro = (RegistroC010)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.Cnpj.Should().Be(default(Cnpj));
    }

    [Theory]
    [InlineData(IndicadorEscrituracaoNfe.ConsolidacaoPorNfe)]
    [InlineData(IndicadorEscrituracaoNfe.IndividualizadoPorNfe)]
    public async Task RoundTrip_IndEscri_SerializaCodigoCorreto(IndicadorEscrituracaoNfe indicador)
    {
        var sped = $"|C010|11222333000181|{(int)indicador}|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C010|11222333000181|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemIndEscri_PreservaTextoCanonico()
    {
        const string sped = "|C010|11222333000181||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_AninhadoSobreC001_RespeitaHierarquia()
    {
        const string sped =
            "|C001|0|\r\n" +
            "|C010|11222333000181|2|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_MultiplosEstabelecimentos_PreservaOrdem()
    {
        const string sped =
            "|C001|0|\r\n" +
            "|C010|11222333000181|1|\r\n" +
            "|C010|33444555000181|2|\r\n";

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

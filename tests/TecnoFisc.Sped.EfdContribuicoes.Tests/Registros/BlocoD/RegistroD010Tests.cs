using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoD;

public sealed class RegistroD010Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD010).Assembly);

    [Fact]
    public void Atributo_DeclaraCodigoD010_Nivel2_BlocoD()
    {
        var atributo = typeof(RegistroD010).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D010");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD010ComUmCampoNaOrdem()
    {
        _catalogo.TentarObter("D010".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D010");
        meta.Campos.Select(c => c.Nome).Should().Equal(["Cnpj"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2]);
        meta.Campos[0].Tamanho.Should().Be(14);
        meta.Campos[0].Obrigatorio.Should().BeTrue();
    }

    [Fact]
    public void Definidor_AtribuiCnpj()
    {
        _catalogo.TentarObter("D010".AsSpan(), out var meta);
        var registro = (RegistroD010)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "11222333000181".AsSpan());

        registro.Cnpj.ToString().Should().Be("11222333000181");
    }

    [Fact]
    public void Definidor_CampoVazio_SetaCnpjDefault()
    {
        // Cnpj é um value type não anulável (Obrigatorio = true); o catálogo retorna
        // default(Cnpj) quando o span está vazio, em vez de null.
        _catalogo.TentarObter("D010".AsSpan(), out var meta);
        var registro = (RegistroD010)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.Cnpj.Should().Be(default(Cnpj));
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|D010|11222333000181|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_AninhadoSobreD001_RespeitaHierarquia()
    {
        const string sped =
            "|D001|0|\r\n" +
            "|D010|11222333000181|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_MultiplosEstabelecimentos_PreservaOrdem()
    {
        const string sped =
            "|D001|0|\r\n" +
            "|D010|11222333000181|\r\n" +
            "|D010|33444555000181|\r\n";

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

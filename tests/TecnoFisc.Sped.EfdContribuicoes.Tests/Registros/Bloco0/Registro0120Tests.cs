using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco0;

public sealed class Registro0120Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0120).Assembly);

    [Fact]
    public void Atributo_DeclaraCodigo0120_Nivel2_Bloco0()
    {
        var atributo = typeof(Registro0120).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0120");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0120ComDoisCamposNaOrdem()
    {
        _catalogo.TentarObter("0120".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0120");
        meta.Campos.Select(c => c.Nome).Should().Equal(["MesRefer", "InfComp"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Catalogo_AmbosCamposSaoObrigatorios()
    {
        _catalogo.TentarObter("0120".AsSpan(), out var meta);

        meta!.Campos.Should().AllSatisfy(c => c.Obrigatorio.Should().BeTrue());
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0120".AsSpan(), out var meta);
        var registro = (Registro0120)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "012025".AsSpan());
        meta.Campos[1].Definidor(registro, "04".AsSpan());

        registro.MesRefer.Should().Be("012025");
        registro.InfComp.Should().Be("04");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("0120".AsSpan(), out var meta);
        var registro = (Registro0120)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.InfComp.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // PJ sem operações geradoras de receitas (indicador 04) em dezembro/2024.
        const string sped = "|0120|122024|04|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_PJImunaIsenta_PreservaTextoCanonico()
    {
        // PJ imune ou isenta do IRPJ (indicador 01).
        const string sped = "|0120|012025|01|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_AninhadoSob0001_RespeitaHierarquia()
    {
        const string sped =
            "|0000|006|0|||01122024|31122024|EMPRESA SA|11222333000181|SP|3550308||00|2|\r\n" +
            "|0001|0|\r\n" +
            "|0120|122024|04|\r\n";

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

using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco0;

public sealed class Registro0208Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0208).Assembly);

    [Fact]
    public void Atributo_Declara0208_Nivel4_Bloco0()
    {
        var atributo = typeof(Registro0208).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0208");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0208Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("0208".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0208");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodTab",
            "CodGru",
            "MarcaCom",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0208".AsSpan(), out var meta);
        var registro = (Registro0208)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01".AsSpan());
        meta.Campos[1].Definidor(registro, "SN".AsSpan());
        meta.Campos[2].Definidor(registro, "MARCA XYZ".AsSpan());

        registro.CodTab.Should().Be("01");
        registro.CodGru.Should().Be("SN");
        registro.MarcaCom.Should().Be("MARCA XYZ");
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|0208|01|SN|MARCA XYZ|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CodigoTabela13_PreservaTextoCanonico()
    {
        const string sped = "|0208|13|02|OUTRA MARCA|\r\n";

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

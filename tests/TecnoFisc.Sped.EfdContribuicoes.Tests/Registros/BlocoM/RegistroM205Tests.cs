using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoM;

public sealed class RegistroM205Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroM205).Assembly);

    [Fact]
    public void Atributo_DeclaraM205_Nivel3_BlocoM()
    {
        var atributo = typeof(RegistroM205).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("M205");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("M");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroM205Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("M205".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("M205");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
        meta.Campos.Select(c => c.Nome).Should().Equal(["NumCampo", "CodRec", "VlDebito"]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // NumCampo
        meta.Campos[1].Tamanho.Should().Be(6);
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // CodRec
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // VlDebito
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("M205".AsSpan(), out var meta);
        var registro = (RegistroM205)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "08".AsSpan());             // NumCampo
        meta.Campos[1].Definidor(registro, "610603".AsSpan());          // CodRec
        meta.Campos[2].Definidor(registro, "1150,00".AsSpan());         // VlDebito

        registro.NumCampo.Should().Be("08");
        registro.CodRec.Should().Be("610603");
        registro.VlDebito.Should().Be(1150m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|M205|08|610603|1150,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_RegimeCumulativo_PreservaTextoCanonico()
    {
        // Campo 12 (regime cumulativo) com código de receita diferente
        const string sped = "|M205|12|610604|3350,25|\r\n";

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

using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco9;

public sealed class Registro9900Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro9900).Assembly);

    [Fact]
    public void Atributo_Declara9900_Nivel2_Bloco9()
    {
        var atributo = typeof(Registro9900).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("9900");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("9");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro9900Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("9900".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("9900");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
        meta.Campos.Select(c => c.Nome).Should().Equal(["RegBlc", "QtdRegBlc"]);
        meta.Campos[0].Tamanho.Should().Be(4);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // RegBlc
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // QtdRegBlc
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("9900".AsSpan(), out var meta);
        var registro = (Registro9900)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0000".AsSpan());  // RegBlc
        meta.Campos[1].Definidor(registro, "1".AsSpan());     // QtdRegBlc

        registro.RegBlc.Should().Be("0000");
        registro.QtdRegBlc.Should().Be(1);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|9900|0000|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_RegistroC100ComMuitasOcorrencias_PreservaTextoCanonico()
    {
        const string sped = "|9900|C100|1500|\r\n";

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

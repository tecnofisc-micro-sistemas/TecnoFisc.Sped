using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoI;

public sealed class RegistroI300Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroI300).Assembly);

    [Fact]
    public void Atributo_DeclaraI300_Nivel5_BlocoI()
    {
        var atributo = typeof(RegistroI300).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I300");
        atributo.Nivel.Should().Be(5);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI300Com4CamposNaOrdem()
    {
        _catalogo.TentarObter("I300".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I300");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodComp", "DetValor", "CodCta", "InfoCompl"]);
        meta.Campos[0].Tamanho.Should().Be(60);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // CodComp
        meta.Campos[2].Tamanho.Should().Be(255);        // CodCta
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I300".AsSpan(), out var meta);
        var registro = (RegistroI300)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "A010101".AsSpan());               // CodComp
        meta.Campos[1].Definidor(registro, "50000,00".AsSpan());              // DetValor
        meta.Campos[2].Definidor(registro, "1.01.01.001.0001".AsSpan());      // CodCta
        meta.Campos[3].Definidor(registro, "Detalhe analítico".AsSpan());     // InfoCompl

        registro.CodComp.Should().Be("A010101");
        registro.DetValor.Should().Be(50000m);
        registro.CodCta.Should().Be("1.01.01.001.0001");
        registro.InfoCompl.Should().Be("Detalhe analítico");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("I300".AsSpan(), out var meta);
        var registro = (RegistroI300)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty); // DetValor
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // InfoCompl

        registro.DetValor.Should().BeNull();
        registro.CodCta.Should().BeNull();
        registro.InfoCompl.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|I300|A010101|50000,00|1.01.01.001.0001|Detalhe analítico|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCodCtaEInfoCompl_PreservaTextoCanonico()
    {
        const string sped = "|I300|B020101|12500,00|||\r\n";

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

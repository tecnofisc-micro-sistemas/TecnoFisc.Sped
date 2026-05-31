using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoP;

public sealed class RegistroP110Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroP110).Assembly);

    [Fact]
    public void Atributo_DeclaraP110_Nivel4_BlocoP()
    {
        var atributo = typeof(RegistroP110).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("P110");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("P");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroP110Com4CamposNaOrdem()
    {
        _catalogo.TentarObter("P110".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("P110");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
        meta.Campos.Select(c => c.Nome).Should().Equal(["NumCampo", "CodDet", "DetValor", "InfCompl"]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // NumCampo
        meta.Campos[1].Tamanho.Should().Be(8);
        meta.Campos[1].Obrigatorio.Should().BeFalse();  // CodDet
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // DetValor
        meta.Campos[3].Obrigatorio.Should().BeFalse();  // InfCompl
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("P110".AsSpan(), out var meta);
        var registro = (RegistroP110)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "06".AsSpan());                          // NumCampo
        meta.Campos[1].Definidor(registro, "00000001".AsSpan());                    // CodDet
        meta.Campos[2].Definidor(registro, "75000,00".AsSpan());                    // DetValor
        meta.Campos[3].Definidor(registro, "Detalhamento receita ativa".AsSpan()); // InfCompl

        registro.NumCampo.Should().Be("06");
        registro.CodDet.Should().Be("00000001");
        registro.DetValor.Should().Be(75000m);
        registro.InfCompl.Should().Be("Detalhamento receita ativa");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("P110".AsSpan(), out var meta);
        var registro = (RegistroP110)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty); // CodDet
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // InfCompl

        registro.CodDet.Should().BeNull();
        registro.InfCompl.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|P110|06|00000001|75000,00|Detalhamento receita ativa|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|P110|06||75000,00||\r\n";

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

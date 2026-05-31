using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoI;

public sealed class RegistroI010Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroI010).Assembly);

    [Fact]
    public void Atributo_DeclaraI010_Nivel2_BlocoI()
    {
        var atributo = typeof(RegistroI010).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I010");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI010Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("I010".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I010");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
        meta.Campos.Select(c => c.Nome).Should().Equal(["Cnpj", "IndAtiv", "InfoCompl"]);
        meta.Campos[0].Tamanho.Should().Be(14);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // Cnpj
        meta.Campos[1].Tamanho.Should().Be(2);
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // IndAtiv
        meta.Campos[2].Obrigatorio.Should().BeFalse();  // InfoCompl
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I010".AsSpan(), out var meta);
        var registro = (RegistroI010)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "12345678000195".AsSpan()); // Cnpj
        meta.Campos[1].Definidor(registro, "01".AsSpan());             // IndAtiv
        meta.Campos[2].Definidor(registro, "Complemento".AsSpan());    // InfoCompl

        registro.Cnpj.Should().Be(Cnpj.Create("12345678000195"));
        registro.IndAtiv.Should().Be(IndicadorAtividadeInstituicaoFinanceira.InstituicoesFinanceiras);
        registro.InfoCompl.Should().Be("Complemento");
    }

    [Fact]
    public void Definidor_CampoOpcionalVazio_DevolveNulo()
    {
        _catalogo.TentarObter("I010".AsSpan(), out var meta);
        var registro = (RegistroI010)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // InfoCompl

        registro.InfoCompl.Should().BeNull();
    }

    [Theory]
    [InlineData("01", IndicadorAtividadeInstituicaoFinanceira.InstituicoesFinanceiras)]
    [InlineData("02", IndicadorAtividadeInstituicaoFinanceira.SegurosPrivados)]
    [InlineData("03", IndicadorAtividadeInstituicaoFinanceira.PrevidenciaComplementar)]
    [InlineData("04", IndicadorAtividadeInstituicaoFinanceira.Capitalizacao)]
    [InlineData("05", IndicadorAtividadeInstituicaoFinanceira.PlanosAssistenciaSaude)]
    [InlineData("06", IndicadorAtividadeInstituicaoFinanceira.MultiplosTipos)]
    public void Definidor_IndAtiv_AtribuiEnumCorreto(string codigo, IndicadorAtividadeInstituicaoFinanceira esperado)
    {
        _catalogo.TentarObter("I010".AsSpan(), out var meta);
        var registro = (RegistroI010)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, codigo.AsSpan());

        registro.IndAtiv.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|I010|12345678000195|01|Complemento técnico|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemInfoCompl_PreservaTextoCanonico()
    {
        const string sped = "|I010|11222333000181|03||\r\n";

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

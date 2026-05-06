using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco1;

public sealed class Registro1020Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1020).Assembly);

    [Fact]
    public void Atributo_Declara1020_Nivel2_Bloco1()
    {
        var atributo = typeof(Registro1020).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1020");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1020Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("1020".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1020");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "NumProc", "IndNatAcao", "DtDecAdm",
        ]);
        meta.Campos[0].Tamanho.Should().Be(20);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // NumProc
        meta.Campos[1].Tamanho.Should().Be(2);
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // IndNatAcao
        meta.Campos[2].Tamanho.Should().Be(8);
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // DtDecAdm
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1020".AsSpan(), out var meta);
        var registro = (Registro1020)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "PA/2021/00012345".AsSpan());  // NumProc
        meta.Campos[1].Definidor(registro, "01".AsSpan());                // IndNatAcao
        meta.Campos[2].Definidor(registro, "15012021".AsSpan());          // DtDecAdm

        registro.NumProc.Should().Be("PA/2021/00012345");
        registro.IndNatAcao.Should().Be(IndicadorNaturezaAcaoAdministrativa.ProcessoAdministrativoDeConsulta);
        registro.DtDecAdm.Should().Be(new DateOnly(2021, 1, 15));
    }

    [Theory]
    [InlineData("01", IndicadorNaturezaAcaoAdministrativa.ProcessoAdministrativoDeConsulta)]
    [InlineData("02", IndicadorNaturezaAcaoAdministrativa.DespachoDecisorio)]
    [InlineData("03", IndicadorNaturezaAcaoAdministrativa.AtoDeclaratorioExecutivo)]
    [InlineData("04", IndicadorNaturezaAcaoAdministrativa.AtoDeclaratorioInterpretativo)]
    [InlineData("05", IndicadorNaturezaAcaoAdministrativa.DecisaoAdministrativaDrjOuCarf)]
    [InlineData("06", IndicadorNaturezaAcaoAdministrativa.AutoDeInfracao)]
    [InlineData("99", IndicadorNaturezaAcaoAdministrativa.Outros)]
    public void Definidor_IndNatAcao_AtribuiEnumCorreto(string codigo, IndicadorNaturezaAcaoAdministrativa esperado)
    {
        _catalogo.TentarObter("1020".AsSpan(), out var meta);
        var registro = (Registro1020)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, codigo.AsSpan());

        registro.IndNatAcao.Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorNaturezaAcaoAdministrativa.ProcessoAdministrativoDeConsulta, "01")]
    [InlineData(IndicadorNaturezaAcaoAdministrativa.DespachoDecisorio, "02")]
    [InlineData(IndicadorNaturezaAcaoAdministrativa.AtoDeclaratorioExecutivo, "03")]
    [InlineData(IndicadorNaturezaAcaoAdministrativa.AtoDeclaratorioInterpretativo, "04")]
    [InlineData(IndicadorNaturezaAcaoAdministrativa.DecisaoAdministrativaDrjOuCarf, "05")]
    [InlineData(IndicadorNaturezaAcaoAdministrativa.AutoDeInfracao, "06")]
    [InlineData(IndicadorNaturezaAcaoAdministrativa.Outros, "99")]
    public void Serializar_IndNatAcao_RetornaCodigoSpedCorreto(IndicadorNaturezaAcaoAdministrativa natureza, string esperado)
    {
        _catalogo.TentarObter("1020".AsSpan(), out var meta);
        var registro = (Registro1020)meta!.Fabrica();
        registro.IndNatAcao = natureza;

        meta.Campos[1].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1020|PA/2021/00012345|01|15012021|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComDecisaoCarf_PreservaTextoCanonico()
    {
        const string sped = "|1020|CARF-2022-000456|05|30062022|\r\n";

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

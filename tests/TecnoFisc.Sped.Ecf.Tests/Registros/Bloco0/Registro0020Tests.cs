using System.Reflection;

using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Ecf.Tests.Versionamento;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.Bloco0;

public sealed class Registro0020Tests
{
    private const string LinhaSemCebas =
        "|0020|1|1|N|N|S|N|N|S|N|N|N|N|S|N|S|N|N|N|N|N|N|N|N|N|N|N|N|N|N|N||";

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new Registro0020(), "0020", "1:1");
    }

    [Fact]
    public void Parser_LeContagemEIndicadoresSemExigirCebasCondicional()
    {
        var resultado = new ParserEcf().ParseLinha(LinhaSemCebas);

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<Registro0020>().Which;
        registro.IndAliqCsll.Should().Be("1");
        registro.IndQteScp.Should().Be(1);
        registro.IndAdmFunClu.Should().Be(IndicadorSimNao.Nao);
        registro.IndOpExt.Should().Be(IndicadorSimNao.Sim);
        registro.IndicadorPosicao31.Should().Be(IndicadorSimNao.Nao);
        registro.Cebas.Should().BeNull();
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Campo31_MapeiaComoUnicaPortaPosicionalSemAliasesSemanticos()
    {
        var registro = new Registro0020
        {
            IndicadorPosicao31 = IndicadorSimNao.Sim,
        };

        registro.IndicadorPosicao31.Should().Be(IndicadorSimNao.Sim);

        var mapeamento = typeof(Registro0020).GetProperties()
            .Select(propriedade => (
                Propriedade: propriedade,
                Campo: propriedade.GetCustomAttribute<CampoSpedAttribute>()))
            .Single(item => item.Campo?.Ordem == 31);
        mapeamento.Propriedade.Name.Should().Be(nameof(Registro0020.IndicadorPosicao31));
        mapeamento.Campo!.Nome.Should().Be("POSSUI_CEBRAS");
        mapeamento.Campo.DesdeVersao.Should().Be(10);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(11)]
    public async Task IndPrTransf_RespondeSomenteNosLeiautes10E11(int versao)
    {
        var registro = await Ler0020(versao);

        registro.IndPrTransf.Should().Be(registro.IndicadorPosicao31);
        registro.PossuiCebras.Should().BeNull();
    }

    [Fact]
    public async Task PossuiCebras_RespondeSomenteDoLeiaute12EmDiante()
    {
        var registro = await Ler0020(12);

        registro.PossuiCebras.Should().Be(registro.IndicadorPosicao31);
        registro.IndPrTransf.Should().BeNull();
    }

    [Fact]
    public async Task VersaoDoArquivo_EhPropagadaParaTodoRegistroLido_InclusiveO0000()
    {
        var registros = await FixtureEcf.ReadAsync(11, "|0001|0|");

        registros.Should().NotBeEmpty();
        registros.Should().OnlyContain(registro => registro.VersaoDoArquivo == 11);
    }

    /// <summary>
    /// Monta um 0020 com as 30 primeiras colunas de dado (Ordem 2..31), a última sendo o campo
    /// posicional 31 — ativo a partir do leiaute 10.
    /// </summary>
    private static async Task<Registro0020> Ler0020(int versao)
    {
        var valores = new List<string> { "1", "1" };
        valores.AddRange(Enumerable.Repeat("N", 27));
        valores.Add("S");
        string linha = "|0020|" + string.Join('|', valores) + "|";
        return (await FixtureEcf.ReadAsync(versao, linha)).OfType<Registro0020>().Single();
    }
}

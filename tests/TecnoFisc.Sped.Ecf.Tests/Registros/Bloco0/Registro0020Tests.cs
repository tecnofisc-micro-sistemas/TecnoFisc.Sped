using System.Reflection;

using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
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
        registro.PossuiCebras.Should().Be(IndicadorSimNao.Nao);
        registro.Cebas.Should().BeNull();
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Campo31_PreservaPosicaoComAliasesSemanticosSemDuplicarMapeamento()
    {
        var registro = new Registro0020
        {
            IndicadorPosicao31 = IndicadorSimNao.Sim,
        };

        registro.IndPrTransf.Should().Be(IndicadorSimNao.Sim,
            "essa é a semântica da posição 31 nos leiautes 10 e 11");
        registro.PossuiCebras.Should().Be(IndicadorSimNao.Sim,
            "essa é a semântica da mesma posição no leiaute 12");

        var mapeamento = typeof(Registro0020).GetProperties()
            .Select(propriedade => (
                Propriedade: propriedade,
                Campo: propriedade.GetCustomAttribute<CampoSpedAttribute>()))
            .Single(item => item.Campo?.Ordem == 31);
        mapeamento.Propriedade.Name.Should().Be(nameof(Registro0020.IndicadorPosicao31));
        mapeamento.Campo!.Nome.Should().Be("POSSUI_CEBRAS");
        mapeamento.Campo.DesdeVersao.Should().Be(10);
    }

    [Fact]
    public void AliasesDoCampo31_CompartilhamOMesmoValorPosicional()
    {
        var registro = new Registro0020
        {
            IndPrTransf = IndicadorSimNao.Nao,
        };

        registro.IndicadorPosicao31.Should().Be(IndicadorSimNao.Nao);
        registro.PossuiCebras = IndicadorSimNao.Sim;
        registro.IndicadorPosicao31.Should().Be(IndicadorSimNao.Sim);
        registro.IndPrTransf.Should().Be(IndicadorSimNao.Sim);
    }
}

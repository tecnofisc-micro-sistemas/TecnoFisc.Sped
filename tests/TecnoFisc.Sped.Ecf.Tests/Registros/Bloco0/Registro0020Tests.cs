using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
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
}

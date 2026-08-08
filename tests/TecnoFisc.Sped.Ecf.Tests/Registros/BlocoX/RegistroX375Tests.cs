using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote2;

public sealed class RegistroX375Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX375(), "X375", "0:N");
    }

    [Fact]
    public void Parser_PreservaInformacaoDinamicaDoMetodoSemInterpretarDominio()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X375|000003|PARAMETRO DO METODO|FAIXA A; USD; -12,3456%|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX375>().Which;
        registro.CampoCodigo.Should().Be("000003");
        registro.Descricao.Should().Be("PARAMETRO DO METODO");
        registro.Valor.Should().Be("FAIXA A; USD; -12,3456%");
        registro.ErrosDeFormato.Should().BeEmpty();
    }
}

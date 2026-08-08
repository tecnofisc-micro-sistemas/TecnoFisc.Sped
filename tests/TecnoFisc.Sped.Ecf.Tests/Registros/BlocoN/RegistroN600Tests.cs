using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoN;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoN.Lote1;

public sealed class RegistroN600Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroN600(), "N600", "0:N");
    }

    [Fact]
    public void Parser_PreservaLinhaDaTabelaDinamicaComoDadosTextuais()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|N600|0066|PARCELA DAS DEMAIS ATIVIDADES|10000,00|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroN600>().Which;
        registro.CampoCodigo.Should().Be("0066");
        registro.Descricao.Should().Be("PARCELA DAS DEMAIS ATIVIDADES");
        registro.Valor.Should().Be("10000,00");
    }

    [Fact]
    public void Parser_ValorTextualComSinalENivelDePrecisao_PreservaRepresentacao()
    {
        var resultado = new ParserEcf().ParseLinha("|N600|0050||-00010,5000|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroN600>()
            .Which.Valor.Should().Be("-00010,5000");
    }
}

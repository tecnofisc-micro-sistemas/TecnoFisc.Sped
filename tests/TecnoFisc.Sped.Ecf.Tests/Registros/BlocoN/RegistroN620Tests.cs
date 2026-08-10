using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoN;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoN.Lote1;

public sealed class RegistroN620Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroN620(), "N620", "0:N");
    }

    [Fact]
    public void Parser_PreservaCodigoDescricaoEValorTextualMensal()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|N620|0007|OPERACOES DE CARATER CULTURAL|10000,00|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroN620>().Which;
        registro.CampoCodigo.Should().Be("0007");
        registro.Descricao.Should().Be("OPERACOES DE CARATER CULTURAL");
        registro.Valor.Should().Be("10000,00");
    }

    [Fact]
    public void Parser_CodigoComZerosEOpcionaisVazios_PreservaRepresentacao()
    {
        var resultado = new ParserEcf().ParseLinha("|N620|0007|||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroN620>().Which;
        registro.CampoCodigo.Should().Be("0007");
        registro.Descricao.Should().BeNull();
        registro.Valor.Should().BeNull();
    }
}

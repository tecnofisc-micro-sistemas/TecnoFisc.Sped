using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoY;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoY.Lote2;

public sealed class RegistroY660Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistroY660()
    {
        AssertRegistroEcf.CodesAreImplemented("Y660");
    }

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroY660(), "Y660", "0:N");
    }

    [Fact]
    public void Parser_LeCnpjNomeLongoEPercentualComEscalaQuatro()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|Y660|22222222000191|EMPRESA SUCESSORA COM NOME SEM LIMITE FIXO|40,1250|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroY660>().Which;
        registro.Cnpj.Should().Be(Cnpj.Create("22222222000191"));
        registro.NomEmp.Should().Be("EMPRESA SUCESSORA COM NOME SEM LIMITE FIXO");
        registro.PercPatLiq.Should().Be(40.1250m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_CnpjEPercentualInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|Y660|INVALIDO|SUCESSORA|PERCENTUAL|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroY660>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([nameof(RegistroY660.Cnpj), nameof(RegistroY660.PercPatLiq)]);
    }
}

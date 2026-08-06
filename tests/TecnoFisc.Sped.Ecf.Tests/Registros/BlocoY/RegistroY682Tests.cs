using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoY;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoY.Lote2;

public sealed class RegistroY682Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistroY682()
    {
        AssertRegistroEcf.CodesAreImplemented("Y682");
    }

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroY682(), "Y682", "0:12");
    }

    [Fact]
    public void Parser_LeMesEAcrescimoPatrimonialAssinado()
    {
        var resultado = new ParserEcf().ParseLinha("|Y682|11|-2345678,10|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroY682>().Which;
        registro.Mes.Should().Be(MesCalendarioEcf.Novembro);
        registro.AcresPatr.Should().Be(-2345678.10m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_MesEValorInvalidos_RegistramErrosDeFormato()
    {
        var registro = new ParserEcf().ParseLinha("|Y682|00|INVALIDO|").Valor
            .Should().BeOfType<RegistroY682>().Which;

        registro.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([nameof(RegistroY682.Mes), nameof(RegistroY682.AcresPatr)]);
    }
}

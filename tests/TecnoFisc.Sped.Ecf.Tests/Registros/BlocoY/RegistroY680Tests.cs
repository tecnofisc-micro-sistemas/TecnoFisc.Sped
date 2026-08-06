using System.Reflection;

using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoY;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoY.Lote2;

public sealed class RegistroY680Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistroY680()
    {
        AssertRegistroEcf.CodesAreImplemented("Y680");
    }

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroY680(), "Y680", "0:12");
    }

    [Fact]
    public void DominioMes_CoincideComOsDozeMesesDaTabelaCompleta()
    {
        typeof(MesCalendarioEcf).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(campo => campo.GetCustomAttribute<SpedValorAttribute>()!.Valor)
            .Should().Equal("01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12");
    }

    [Theory]
    [InlineData("01", MesCalendarioEcf.Janeiro)]
    [InlineData("12", MesCalendarioEcf.Dezembro)]
    public void Parser_LeMesComZeroSignificativo(string token, MesCalendarioEcf esperado)
    {
        var resultado = new ParserEcf().ParseLinha($"|Y680|{token}|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroY680>().Which.Mes.Should().Be(esperado);
    }

    [Fact]
    public void Parser_MesInvalido_RegistraErroDeFormato()
    {
        var registro = new ParserEcf().ParseLinha("|Y680|13|").Valor
            .Should().BeOfType<RegistroY680>().Which;

        registro.ErrosDeFormato.Should().ContainSingle(erro => erro.Campo == nameof(RegistroY680.Mes));
    }
}

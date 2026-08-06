using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.Bloco0;

public sealed class Registro0035Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new Registro0035(), "0035", "0:N");
    }

    [Fact]
    public void Parser_LeCnpjDaScp()
    {
        var resultado = new ParserEcf().ParseLinha("|0035|11222333000181|SCP SINTETICA|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<Registro0035>().Which;
        registro.CodScp.Should().Be(Cnpj.Create("11222333000181"));
        registro.NomeScp.Should().Be("SCP SINTETICA");
    }

    [Fact]
    public void Parser_CnpjInvalido_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|0035|INVALIDO|SCP SINTETICA|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<Registro0035>().Which;
        registro.ErrosDeFormato.Should().ContainSingle(erro =>
            erro.Campo == "CodScp" && erro.ValorBruto == "INVALIDO");
    }
}

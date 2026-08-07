using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoN;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoN.Lote1;

public sealed class RegistroN605Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroN605(), "N605", "0:N");
    }

    [Fact]
    public void Parser_LeContaCentroOpcionalValorComSinalEIndicador()
    {
        var resultado = new ParserEcf().ParseLinha("|N605|000111||-10000,25|D|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroN605>().Which;
        registro.CodCta.Should().Be("000111");
        registro.CodCcus.Should().BeNull();
        registro.Valor.Should().Be(-10000.25m);
        registro.IndValor.Should().Be(IndicadorDebitoCredito.Devedor);
    }

    [Fact]
    public void Parser_ValorEIndicadorInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|N605|000111|0007|INVALIDO|X|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroN605>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain(["VALOR", "IND_VALOR"]);
    }
}
